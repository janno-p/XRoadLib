using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using XRoadLib.Events;
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

            await context.Request.LoadRequestAsync(context.HttpContext, context.MessageFormatter, GetStorageOrTempPath().FullName, ServiceManager).ConfigureAwait(false);
            context.Response.Copy(context.Request);

            context.HttpContext.Response.ContentType = context.MessageFormatter.ContentType;

            await OnRequestLoadedAsync(context).ConfigureAwait(false);
            await InvokeWebServiceAsync(context).ConfigureAwait(false);
            await SerializeXRoadResponseAsync(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Handle X-Road message protocol meta-service request.
        /// </summary>
        protected virtual Task<object> InvokeMetaServiceAsync(WebServiceContext context) =>
            Task.FromResult<object>(null);

        /// <summary>
        /// Get main service object which implements the functionality of
        /// the operation.
        /// </summary>
        protected virtual Task<object> GetServiceObjectAsync(WebServiceContext context)
        {
            var operationDefinition = context.ServiceMap.OperationDefinition;

            var service = operationDefinition.MethodInfo.DeclaringType != null
                ? ServiceProvider.GetRequiredService(operationDefinition.MethodInfo.DeclaringType)
                : null;

            if (service == null)
                throw new SchemaDefinitionException($"Operation {operationDefinition.Name} is not implemented by contract.");

            return Task.FromResult(service);
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
        protected virtual Task OnBeforeDeserializationAsync(WebServiceContext context, BeforeDeserializationEventArgs args) =>
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
            if (context.ServiceMap != null)
            {
                context.Result = await InvokeMetaServiceAsync(context).ConfigureAwait(false);
                return;
            }

            var operationName = ResolveOperationName(context);

            context.ServiceMap = context.Request.GetSerializer().GetServiceMap(operationName);
            context.Response.BinaryMode = context.ServiceMap.OperationDefinition.OutputBinaryMode;

            var serviceObject = await GetServiceObjectAsync(context).ConfigureAwait(false);
            await DeserializeMethodInputAsync(context).ConfigureAwait(false);

            try
            {
                var parameters = context.ServiceMap.RequestDefinition.ParameterInfo != null ? new[] { context.Parameters } : new object[0];
                context.Result = await InvokeRuntimeMethodAsync(context, serviceObject, parameters).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                context.Exception = exception;
                await OnInvocationErrorAsync(context).ConfigureAwait(false);

                if (context.Result == null)
                    throw;
            }
        }

        protected virtual Task<object> InvokeRuntimeMethodAsync(WebServiceContext context, object serviceObject, object[] parameters) =>
            Task.FromResult(context.ServiceMap.OperationDefinition.MethodInfo.Invoke(serviceObject, parameters));

        /// <summary>
        /// Deserializes X-Road request from SOAP message payload.
        /// </summary>
        protected virtual async Task DeserializeMethodInputAsync(WebServiceContext context)
        {
            var args = new BeforeDeserializationEventArgs();
            await OnBeforeDeserializationAsync(context, args).ConfigureAwait(false);

            args.XmlReaderSettings ??= new XmlReaderSettings();
            args.XmlReaderSettings.Async = true;

            context.Request.ContentStream.Position = 0;
            var reader = XmlReader.Create(context.Request.ContentStream, args.XmlReaderSettings);

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