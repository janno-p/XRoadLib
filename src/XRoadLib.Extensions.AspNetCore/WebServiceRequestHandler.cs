using System;
using System.IO;
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
        public override void HandleRequest(WebServiceContext context)
        {
            if (context.HttpContext.Request.Body.CanSeek && context.HttpContext.Request.Body.Length == 0)
                throw new InvalidQueryException("Empty request content");

            context.MessageFormatter = context.Request.LoadRequest(context.HttpContext, GetStorageOrTempPath().FullName, ServiceManager);

            context.Response.Copy(context.Request);
            context.ServiceMap = context.Request.MetaServiceMap;

            context.HttpContext.Response.ContentType = context.MessageFormatter.ContentType;

            OnRequestLoaded(context);
            InvokeServiceMethod(context);
            SerializeXRoadResponse(context);
        }

        /// <summary>
        /// Handle X-Road message protocol meta-service request.
        /// </summary>
        protected virtual object InvokeMetaService(WebServiceContext context)
        {
            return null;
        }

        /// <summary>
        /// Get main service object which implements the functionality of
        /// the operation.
        /// </summary>
        protected virtual object GetServiceObject(WebServiceContext context)
        {
            var operationDefinition = context.ServiceMap.OperationDefinition;

            var service = serviceProvider.GetRequiredService(operationDefinition.MethodInfo.DeclaringType);

            return service ?? throw new SchemaDefinitionException($"Operation {operationDefinition.Name} is not implemented by contract.");
        }

        /// <summary>
        /// Intercept X-Road service request after request message is loaded.
        /// </summary>
        protected virtual void OnRequestLoaded(WebServiceContext context)
        { }

        /// <summary>
        /// Handle exception that occured on service method invokation.
        /// </summary>
        protected virtual void OnInvocationError(WebServiceContext context)
        { }

        /// <summary>
        /// Customize XML reader settings before deserialization of the X-Road message.
        /// </summary>
        protected virtual void OnBeforeDeserialization(WebServiceContext context, BeforeDeserializationEventArgs args)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after deserialization of the message.
        /// </summary>
        protected virtual void OnAfterDeserialization(WebServiceContext context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling before serialization of the response message.
        /// </summary>
        protected virtual void OnBeforeSerialization(WebServiceContext context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after serialization of the response message.
        /// </summary>
        protected virtual void OnAfterSerialization(WebServiceContext context)
        { }

        protected virtual XName ResolveOperationName(WebServiceContext context)
        {
            return context.Request.RootElementName;
        }

        /// <summary>
        /// Calls service method which implements the X-Road operation.
        /// </summary>
        protected virtual void InvokeServiceMethod(WebServiceContext context)
        {
            if (context.ServiceMap != null)
            {
                context.Result = InvokeMetaService(context);
                return;
            }

            var operationName = ResolveOperationName(context);

            context.ServiceMap = context.Request.GetSerializer().GetServiceMap(operationName);
            context.Response.BinaryMode = context.ServiceMap.OperationDefinition.OutputBinaryMode;

            var serviceObject = GetServiceObject(context);
            DeserializeMethodInput(context);

            try
            {
                var parameters = context.ServiceMap.RequestDefinition.ParameterInfo != null ? new[] { context.Parameters } : new object[0];
                context.Result = context.ServiceMap.OperationDefinition.MethodInfo.Invoke(serviceObject, parameters);
            }
            catch (Exception exception)
            {
                context.Exception = exception;
                OnInvocationError(context);

                if (context.Result == null)
                    throw;
            }
        }

        /// <summary>
        /// Deserializes X-Road request from SOAP message payload.
        /// </summary>
        protected virtual void DeserializeMethodInput(WebServiceContext context)
        {
            var args = new BeforeDeserializationEventArgs();
            OnBeforeDeserialization(context, args);

            context.Request.ContentStream.Position = 0;
            var reader = XmlReader.Create(context.Request.ContentStream, args.XmlReaderSettings);

            context.MessageFormatter.MoveToPayload(reader, context.Request.RootElementName);

            context.Parameters = context.ServiceMap.DeserializeRequest(reader, context.Request);

            OnAfterDeserialization(context);
        }

        /// <summary>
        /// Serializes service result to a X-Road message response.
        /// </summary>
        protected virtual void SerializeXRoadResponse(WebServiceContext context)
        {
            OnBeforeSerialization(context);

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

                context.ServiceMap.SerializeResponse(writer, context.Result, context.Response, reader);

                writer.WriteEndDocument();
                writer.Flush();
            }

            context.Response.SaveTo(context.HttpContext, context.MessageFormatter);

            OnAfterSerialization(context);
        }

        private DirectoryInfo GetStorageOrTempPath()
        {
            return StoragePath ?? new DirectoryInfo(Path.GetTempPath());
        }
    }
}