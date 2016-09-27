#if !NETSTANDARD1_5

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib.Handler
{
    /// <summary>
    /// Base class of service request handlers.
    /// </summary>
    public abstract class ServiceRequestHandlerBase : ServiceHandlerBase
    {
        private readonly ICollection<XRoadProtocol> supportedProtocols;

        /// <summary>
        /// Temporary file storage location.
        /// </summary>
        public string StoragePath { get; set; }

        /// <summary>
        /// Serialization overrrides.
        /// </summary>
        public ICustomSerialization CustomSerialization { get; set; }

        /// <summary>
        /// Initialize new service request handler with protocols it can handle.
        /// </summary>
        protected ServiceRequestHandlerBase(IEnumerable<XRoadProtocol> supportedProtocols)
        {
            if (supportedProtocols == null)
                throw new ArgumentNullException(nameof(supportedProtocols));
            this.supportedProtocols = new List<XRoadProtocol>(supportedProtocols);
        }

        /// <summary>
        /// Handle X-Road message protocol meta-service request.
        /// </summary>
        protected abstract object InvokeMetaService(XRoadContextClassic context);

        /// <summary>
        /// Get main service object which implements the functionality of
        /// the operation.
        /// </summary>
        protected abstract object GetServiceObject(XRoadContextClassic context);

        /// <summary>
        /// Intercept X-Road service request after request message is loaded.
        /// </summary>
        protected virtual void OnRequestLoaded(XRoadContextClassic context)
        { }

        /// <summary>
        /// Handle exception that occured on service method invokation.
        /// </summary>
        protected virtual void OnInvocationError(XRoadContextClassic context)
        { }

        /// <summary>
        /// Customize XML reader settings before deserialization of the X-Road message.
        /// </summary>
        protected virtual void OnBeforeDeserialization(XRoadContextClassic context, BeforeDeserializationEventArgs args)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after deserialization of the message.
        /// </summary>
        protected virtual void OnAfterDeserialization(XRoadContextClassic context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling before serialization of the response message.
        /// </summary>
        protected virtual void OnBeforeSerialization(XRoadContextClassic context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after serialization of the response message.
        /// </summary>
        protected virtual void OnAfterSerialization(XRoadContextClassic context)
        { }

        /// <summary>
        /// Handle current X-Road operation.
        /// </summary>
        protected override void HandleRequest(XRoadContextClassic context)
        {
            if (context.HttpContext.Request.InputStream.Length == 0)
                throw XRoadException.InvalidQuery("Empty request content");

            context.Request.LoadRequest(context.HttpContext, StoragePath.GetValueOrDefault(Path.GetTempPath()), supportedProtocols);
            if (context.Request.Protocol == null && context.Request.MetaServiceMap == null)
            {
                var supportedProtocolsString = string.Join(", ", supportedProtocols.Select(x => $@"""{x.Name}"""));
                throw XRoadException.InvalidQuery($"Could not detect X-Road message protocol version from request message. Adapter supports following protocol versions: {supportedProtocolsString}.");
            }

            context.Response.Copy(context.Request);
            context.ServiceMap = context.Request.MetaServiceMap;

            OnRequestLoaded(context);
            InvokeServiceMethod(context);
            SerializeXRoadResponse(context);
        }

        private void InvokeServiceMethod(XRoadContextClassic context)
        {
            if (context.ServiceMap != null)
            {
                context.Result = InvokeMetaService(context);
                return;
            }

            context.ServiceMap = context.Request.GetSerializerCache().GetServiceMap(context.Request.RootElementName);
            context.Response.BinaryMode = context.ServiceMap.Definition.OutputBinaryMode;

            var serviceObject = GetServiceObject(context);
            DeserializeMethodInput(context);

            try
            {
                var parameters = context.ServiceMap.HasParameters ? new[] { context.Parameters } : new object[0];
                context.Result = context.ServiceMap.Definition.MethodInfo.Invoke(serviceObject, parameters);
            }
            catch (Exception exception)
            {
                context.Exception = exception;
                OnInvocationError(context);

                if (context.Result == null)
                    throw;
            }
        }

        private void DeserializeMethodInput(XRoadContextClassic context)
        {
            var args = new BeforeDeserializationEventArgs();
            OnBeforeDeserialization(context, args);

            context.Request.ContentStream.Position = 0;
            var reader = XmlReader.Create(context.Request.ContentStream, args.XmlReaderSettings);

            reader.MoveToPayload(context.Request.RootElementName);

            context.Parameters = context.ServiceMap.DeserializeRequest(reader, context.Request);

            OnAfterDeserialization(context);
        }

        private void SerializeXRoadResponse(XRoadContextClassic context)
        {
            OnBeforeSerialization(context);

            context.Request.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(context.Request.ContentStream, new XmlReaderSettings { CloseInput = false }))
            {
                var writer = new XmlTextWriter(context.Response.ContentStream, context.Response.ContentEncoding);

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

                context.ServiceMap.SerializeResponse(writer, context.Result, context.Response, reader, CustomSerialization);

                writer.WriteEndDocument();
                writer.Flush();
            }

            context.Response.SaveTo(context.HttpContext);

            OnAfterSerialization(context);
        }
    }
}

#endif