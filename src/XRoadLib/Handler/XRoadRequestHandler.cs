#if !NET452

using System;
using System.IO;
using System.Xml;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib.Handler
{
    /// <summary>
    /// Base class of service request handlers.
    /// </summary>
    public class XRoadRequestHandler : XRoadHandlerBase
    {
        private readonly DirectoryInfo storagePath;

        /// <summary>
        /// Initialize new service request handler with X-Road message protocols
        /// it should be able to handle and storage path of temporary files.
        /// </summary>
        public XRoadRequestHandler(IServiceManager serviceManager, DirectoryInfo storagePath)
            : base(serviceManager)
        {
            this.storagePath = storagePath ?? new DirectoryInfo(Path.GetTempPath());
        }

        /// <summary>
        /// Handle incoming web request as X-Road service request.
        /// </summary>
        public override void HandleRequest(XRoadContext context)
        {
            if (context.HttpContext.Request.Body.CanSeek && context.HttpContext.Request.Body.Length == 0)
                throw XRoadException.InvalidQuery("Empty request content");

            context.Request.LoadRequest(context.HttpContext, storagePath.FullName, ServiceManager);
            if (context.Request.ServiceManager == null && context.Request.MetaServiceMap == null)
                throw XRoadException.InvalidQuery($"Could not detect X-Road message protocol version from request message. Adapter supports following protocol versions: {ServiceManager.Name}.");

            context.Response.Copy(context.Request);
            context.ServiceMap = context.Request.MetaServiceMap;

            OnRequestLoaded(context);
            InvokeServiceMethod(context);
            SerializeXRoadResponse(context);
        }

        /// <summary>
        /// Handle X-Road message protocol meta-service request.
        /// </summary>
        protected virtual object InvokeMetaService(XRoadContext context)
        {
            return null;
        }

        /// <summary>
        /// Get main service object which implements the functionality of
        /// the operation.
        /// </summary>
        protected virtual object GetServiceObject(XRoadContext context)
        {
            return null;
        }

        /// <summary>
        /// Intercept X-Road service request after request message is loaded.
        /// </summary>
        protected virtual void OnRequestLoaded(XRoadContext context)
        { }

        /// <summary>
        /// Handle exception that occured on service method invokation.
        /// </summary>
        protected virtual void OnInvocationError(XRoadContext context)
        { }

        /// <summary>
        /// Customize XML reader settings before deserialization of the X-Road message.
        /// </summary>
        protected virtual void OnBeforeDeserialization(XRoadContext context, BeforeDeserializationEventArgs args)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after deserialization of the message.
        /// </summary>
        protected virtual void OnAfterDeserialization(XRoadContext context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling before serialization of the response message.
        /// </summary>
        protected virtual void OnBeforeSerialization(XRoadContext context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after serialization of the response message.
        /// </summary>
        protected virtual void OnAfterSerialization(XRoadContext context)
        { }

        /// <summary>
        /// Calls service method which implements the X-Road operation.
        /// </summary>
        protected virtual void InvokeServiceMethod(XRoadContext context)
        {
            if (context.ServiceMap != null)
            {
                context.Result = InvokeMetaService(context);
                return;
            }

            context.ServiceMap = context.Request.GetSerializer().GetServiceMap(context.Request.RootElementName);
            context.Response.BinaryMode = context.ServiceMap.OperationDefinition.OutputBinaryMode;

            var serviceObject = GetServiceObject(context);
            DeserializeMethodInput(context);

            try
            {
                var parameters = context.ServiceMap.RequestDefinition.ParameterInfo != null ? new [] { context.Parameters } : new object[0];
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
        protected virtual void DeserializeMethodInput(XRoadContext context)
        {
            var args = new BeforeDeserializationEventArgs();
            OnBeforeDeserialization(context, args);

            context.Request.ContentStream.Position = 0;
            var reader = XmlReader.Create(context.Request.ContentStream, args.XmlReaderSettings);

            reader.MoveToPayload(context.Request.RootElementName);

            context.Parameters = context.ServiceMap.DeserializeRequest(reader, context.Request);

            OnAfterDeserialization(context);
        }

        /// <summary>
        /// Serializes service result to a X-Road message response.
        /// </summary>
        protected virtual void SerializeXRoadResponse(XRoadContext context)
        {
            OnBeforeSerialization(context);

            context.Request.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(context.Request.ContentStream, new XmlReaderSettings { CloseInput = false }))
            using (var textWriter = new StreamWriter(context.Response.ContentStream, context.Response.ContentEncoding, 1024, true))
            using (var writer = XmlWriter.Create(textWriter))
            {
                writer.WriteStartDocument();

                if (reader.MoveToElement(0) && reader.IsCurrentElement(0, "Envelope", NamespaceConstants.SOAP_ENV))
                {
                    writer.WriteStartElement(reader.Prefix, "Envelope", NamespaceConstants.SOAP_ENV);
                    writer.WriteAttributes(reader, true);
                }
                else
                {
                    writer.WriteStartElement(PrefixConstants.SOAP_ENV, "Envelope", NamespaceConstants.SOAP_ENV);
                    writer.WriteAttributeString("xmlns", PrefixConstants.SOAP_ENV, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENV);
                }

                if (reader.MoveToElement(1) && reader.IsCurrentElement(1, "Header", NamespaceConstants.SOAP_ENV))
                    writer.WriteNode(reader, true);

                writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);
                if (reader.IsCurrentElement(1, "Body", NamespaceConstants.SOAP_ENV) || reader.MoveToElement(1, "Body", NamespaceConstants.SOAP_ENV))
                    writer.WriteAttributes(reader, true);

                context.ServiceMap.SerializeResponse(writer, context.Result, context.Response, reader);

                writer.WriteEndDocument();
                writer.Flush();
            }

            context.Response.SaveTo(context.HttpContext);

            OnAfterSerialization(context);
        }
    }
}

#endif
