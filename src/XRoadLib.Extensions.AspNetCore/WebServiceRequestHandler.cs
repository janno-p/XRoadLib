using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore
{
    /// <summary>
    /// Base class of service request handlers.
    /// </summary>
    public class WebServiceRequestHandler : WebServiceHandler
    {
        protected readonly IServiceProvider ServiceProvider;

        public DirectoryInfo StoragePath { get; set; }

        /// <summary>
        /// Initialize new service request handler with X-Road message protocols
        /// it should be able to handle and storage path of temporary files.
        /// </summary>
        public WebServiceRequestHandler(IServiceProvider serviceProvider, IServiceManager serviceManager)
            : base(serviceManager)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Handle incoming web request as X-Road service request.
        /// </summary>
        public override async Task HandleRequestAsync(WebServiceContext context)
        {
            if (context.HttpContext.Request.Body.CanSeek && context.HttpContext.Request.Body.Length == 0)
                throw new InvalidQueryException("Empty request content");

            await context.Request.LoadRequestAsync(context.HttpContext, context.MessageFormatter, GetStorageOrTempPath(), ServiceManager).ConfigureAwait(false);
            context.Response.Copy(context.Request);

            context.HttpContext.Response.ContentType = context.MessageFormatter.ContentType;

            await OnRequestLoadedAsync(context).ConfigureAwait(false);
            await InvokeWebServiceAsync(context).ConfigureAwait(false);
            await SerializeXRoadResponseAsync(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Intercept X-Road service request after request message is loaded.
        /// </summary>
        protected virtual Task OnRequestLoadedAsync(WebServiceContext context) =>
            Task.CompletedTask;

        /// <summary>
        /// Handle exception that occurred on service method invokation.
        /// </summary>
        protected virtual Task OnInvocationErrorAsync(WebServiceContext context) =>
            Task.CompletedTask;

        /// <summary>
        /// Customize XML reader settings before deserialization of the X-Road message.
        /// </summary>
        protected virtual Task OnBeforeDeserializationAsync(WebServiceContext context, XmlReaderSettings xmlReaderSettings) =>
            Task.CompletedTask;

        /// <summary>
        /// Intercept X-Road service request handling after deserialization of the message.
        /// </summary>
        protected virtual Task OnAfterDeserializationAsync(WebServiceContext context) =>
            Task.CompletedTask;

        /// <summary>
        /// Intercept X-Road service request handling before serialization of the response message.
        /// </summary>
        protected virtual Task OnBeforeSerializationAsync(WebServiceContext context) =>
            Task.CompletedTask;

        /// <summary>
        /// Intercept X-Road service request handling after serialization of the response message.
        /// </summary>
        protected virtual Task OnAfterSerializationAsync(WebServiceContext context) =>
            Task.CompletedTask;

        protected virtual XName ResolveOperationName(WebServiceContext context)
        {
            if (context.Request.RootElementName == null)
                throw new UnknownOperationException("Could not resolve operation name from request.", context.Request.RootElementName);

            return context.Request.RootElementName;
        }

        /// <summary>
        /// Calls service method which implements the X-Road operation.
        /// </summary>
        protected virtual async Task InvokeWebServiceAsync(WebServiceContext context)
        {
            var operationName = ResolveOperationName(context);

            context.ServiceMap = context.Request.GetSerializer().GetServiceMap(operationName);
            context.Response.BinaryMode = context.ServiceMap.OperationDefinition.OutputBinaryMode;

            await DeserializeParametersAsync(context).ConfigureAwait(false);

            try
            {
                context.Result = await InvokeRequestHandlerAsync(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                context.Exception = exception;
                await OnInvocationErrorAsync(context).ConfigureAwait(false);

                if (context.Result == null)
                    throw;
            }
        }

        protected virtual Task<object> InvokeRequestHandlerAsync(WebServiceContext context)
        {
            var handlerFactory = ServiceProvider.GetRequiredService<HandlerFactory>();

            var operation = Activator.CreateInstance(context.ServiceMap.OperationDefinition.OperationType);

            var setRequest = context.ServiceMap.OperationDefinition.OperationType.GetProperty("Request")?.GetSetMethod();
            setRequest?.Invoke(operation, new[] { context.Parameters });

            var setHeader = context.ServiceMap.OperationDefinition.OperationType.GetProperty("Header")?.GetSetMethod();
            setHeader?.Invoke(operation, new object[] { context.Request.Header });
            
            var getAttachments = context.ServiceMap.OperationDefinition.OperationType.GetProperty("Attachments")?.GetGetMethod();
            if (getAttachments != null)
                ((List<XRoadAttachment>)getAttachments.Invoke(operation, new object[0])).AddRange(context.Request.AllAttachments);

            return handlerFactory.Factory(ServiceProvider)(operation);
        }

        /// <summary>
        /// Deserializes X-Road request from SOAP message payload.
        /// </summary>
        protected virtual async Task DeserializeParametersAsync(WebServiceContext context)
        {
            var xmlReaderSettings = new XmlReaderSettings { Async = true };
            await OnBeforeDeserializationAsync(context, xmlReaderSettings).ConfigureAwait(false);

            context.Request.ContentStream.Position = 0;
            var reader = XmlReader.Create(context.Request.ContentStream, xmlReaderSettings);

            await context.MessageFormatter.MoveToPayloadAsync(reader, context.Request.RootElementName).ConfigureAwait(false);

            context.Parameters = await context.ServiceMap.DeserializeRequestAsync(reader, context.Request).ConfigureAwait(false);

            await OnAfterDeserializationAsync(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Serializes service result to a X-Road message response.
        /// </summary>
        protected virtual async Task SerializeXRoadResponseAsync(WebServiceContext context)
        {
            await OnBeforeSerializationAsync(context).ConfigureAwait(false);

            context.Request.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(context.Request.ContentStream, new XmlReaderSettings { Async = true, CloseInput = false }))
            using (var textWriter = new StreamWriter(context.Response.ContentStream, context.Response.ContentEncoding, 1024, true))
            using (var writer = XmlWriter.Create(textWriter, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 }))
            {
                await writer.WriteStartDocumentAsync().ConfigureAwait(false);

                if (await context.MessageFormatter.TryMoveToEnvelopeAsync(reader).ConfigureAwait(false))
                {
                    await context.MessageFormatter.WriteStartEnvelopeAsync(writer, reader.Prefix).ConfigureAwait(false);
                    await writer.WriteAttributesAsync(reader, true).ConfigureAwait(false);
                    await writer.WriteMissingAttributesAsync(ServiceManager.ProtocolDefinition).ConfigureAwait(false);
                }
                else await writer.WriteSoapEnvelopeAsync(context.MessageFormatter, ServiceManager.ProtocolDefinition).ConfigureAwait(false);

                if (await context.MessageFormatter.TryMoveToHeaderAsync(reader).ConfigureAwait(false))
                    await writer.WriteNodeAsync(reader, true).ConfigureAwait(false);

                await context.MessageFormatter.WriteStartBodyAsync(writer).ConfigureAwait(false);
                if (await context.MessageFormatter.TryMoveToBodyAsync(reader).ConfigureAwait(false))
                    await writer.WriteAttributesAsync(reader, true).ConfigureAwait(false);

                await SerializeServiceResultAsync(context, reader, writer).ConfigureAwait(false);

                await writer.WriteEndDocumentAsync().ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            await context.Response.SaveToAsync(context.HttpContext, context.MessageFormatter).ConfigureAwait(false);

            await OnAfterSerializationAsync(context).ConfigureAwait(false);
        }

        protected virtual Task SerializeServiceResultAsync(WebServiceContext context, XmlReader requestReader, XmlWriter responseWriter)
        {
            return context.ServiceMap.SerializeResponseAsync(responseWriter, context.Result, context.Response, requestReader);
        }

        private DirectoryInfo GetStorageOrTempPath()
        {
            return StoragePath ?? new DirectoryInfo(Path.GetTempPath());
        }
    }
}