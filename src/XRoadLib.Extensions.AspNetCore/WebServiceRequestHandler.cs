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
        protected readonly IServiceProvider serviceProvider;

        public DirectoryInfo StoragePath { get; set; }

        /// <summary>
        /// Initialize new service request handler with X-Road message protocols
        /// it should be able to handle and storage path of temporary files.
        /// </summary>
        public WebServiceRequestHandler(IServiceProvider serviceProvider, IServiceManager serviceManager)
            : base(serviceManager)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Handle incoming web request as X-Road service request.
        /// </summary>
        public override async Task HandleRequestAsync(WebServiceContext context)
        {
            if (context.HttpContext.Request.Body.CanSeek && context.HttpContext.Request.Body.Length == 0)
                throw new InvalidQueryException("Empty request content");

            context.Request.LoadRequest(context.HttpContext, context.MessageFormatter, GetStorageOrTempPath().FullName, ServiceManager);
            context.Response.Copy(context.Request);
            context.ServiceMap = context.Request.MetaServiceMap;

            context.HttpContext.Response.ContentType = context.MessageFormatter.ContentType;

            await OnRequestLoadedAsync(context);
            await InvokeWebServiceAsync(context);
            await SerializeXRoadResponseAsync(context);
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

            var service = serviceProvider.GetRequiredService(operationDefinition.MethodInfo.DeclaringType);
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
            return context.Request.RootElementName;
        }

        /// <summary>
        /// Calls service method which implements the X-Road operation.
        /// </summary>
        protected virtual async Task InvokeWebServiceAsync(WebServiceContext context)
        {
            if (context.ServiceMap != null)
            {
                context.Result = await InvokeMetaServiceAsync(context);
                return;
            }

            var operationName = ResolveOperationName(context);

            context.ServiceMap = context.Request.GetSerializer().GetServiceMap(operationName);
            context.Response.BinaryMode = context.ServiceMap.OperationDefinition.OutputBinaryMode;

            var serviceObject = await GetServiceObjectAsync(context);
            await DeserializeMethodInputAsync(context);

            try
            {
                var parameters = context.ServiceMap.RequestDefinition.ParameterInfo != null ? new[] { context.Parameters } : new object[0];
                context.Result = await InvokeRuntimeMethodAsync(context, serviceObject, parameters);
            }
            catch (Exception exception)
            {
                context.Exception = exception;
                await OnInvocationErrorAsync(context);

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
            await OnBeforeDeserializationAsync(context, args);

            context.Request.ContentStream.Position = 0;
            var reader = XmlReader.Create(context.Request.ContentStream, args.XmlReaderSettings);

            context.MessageFormatter.MoveToPayload(reader, context.Request.RootElementName);

            context.Parameters = context.ServiceMap.DeserializeRequest(reader, context.Request);

            await OnAfterDeserializationAsync(context);
        }

        /// <summary>
        /// Serializes service result to a X-Road message response.
        /// </summary>
        protected virtual async Task SerializeXRoadResponseAsync(WebServiceContext context)
        {
            await OnBeforeSerializationAsync(context);

            context.Request.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(context.Request.ContentStream, new XmlReaderSettings { CloseInput = false }))
            using (var textWriter = new StreamWriter(context.Response.ContentStream, context.Response.ContentEncoding, 1024, true))
            using (var writer = XmlWriter.Create(textWriter))
            {
                writer.WriteStartDocument();

                if (context.MessageFormatter.TryMoveToEnvelope(reader))
                {
                    context.MessageFormatter.WriteStartEnvelope(writer, reader.Prefix);
                    writer.WriteAttributes(reader, true);
                    writer.WriteMissingAttributes(ServiceManager.ProtocolDefinition);
                }
                else writer.WriteSoapEnvelope(context.MessageFormatter, ServiceManager.ProtocolDefinition);

                if (context.MessageFormatter.TryMoveToHeader(reader))
                    writer.WriteNode(reader, true);

                context.MessageFormatter.WriteStartBody(writer);
                if (context.MessageFormatter.TryMoveToBody(reader))
                    writer.WriteAttributes(reader, true);

                await SerializeServiceResultAsync(context, reader, writer);

                writer.WriteEndDocument();
                writer.Flush();
            }

            context.Response.SaveTo(context.HttpContext, context.MessageFormatter);

            await OnAfterSerializationAsync(context);
        }

        protected virtual Task SerializeServiceResultAsync(WebServiceContext context, XmlReader requestReader, XmlWriter responseWriter)
        {
            context.ServiceMap.SerializeResponse(responseWriter, context.Result, context.Response, requestReader);
            return Task.CompletedTask;
        }

        private DirectoryInfo GetStorageOrTempPath()
        {
            return StoragePath ?? new DirectoryInfo(Path.GetTempPath());
        }
    }
}