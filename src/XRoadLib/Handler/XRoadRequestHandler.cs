#if !NET40

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.AspNetCore.Http;
using XRoadLib.Extensions;
using XRoadLib.Handler.Events;
using XRoadLib.Protocols;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Handler
{
    public class XRoadRequestHandler : XRoadHandlerBase
    {
        private readonly ICollection<XRoadProtocol> supportedProtocols;
        private readonly string storagePath;

        public XRoadRequestHandler(IEnumerable<XRoadProtocol> supportedProtocols, string storagePath)
        {
            if (supportedProtocols == null)
                throw new ArgumentNullException(nameof(supportedProtocols));

            this.supportedProtocols = supportedProtocols.ToList();
            this.storagePath = string.IsNullOrWhiteSpace(storagePath) ? Path.GetTempPath() : storagePath;
        }

        public override void HandleRequest(HttpContext context)
        {
            using (var requestMessage = new XRoadMessage())
            using (var responseMessage = new XRoadMessage(new MemoryStream()))
            {
                if (context.Request.Body.CanSeek && context.Request.Body.Length == 0)
                    throw XRoadException.InvalidQuery("Empty request content");

                requestMessage.LoadRequest(context, storagePath, supportedProtocols);
                if (requestMessage.Protocol == null && requestMessage.MetaServiceMap == null)
                {
                    var supportedProtocolsString = string.Join(", ", supportedProtocols.Select(x => $@"""{x.Name}"""));
                    throw XRoadException.InvalidQuery($"Could not detect X-Road message protocol version from request message. Adapter supports following protocol versions: {supportedProtocolsString}.");
                }

                responseMessage.Copy(requestMessage);

                OnRequestLoaded();

                IServiceMap serviceMap;
                var result = InvokeServiceMethod(requestMessage, out serviceMap);

                responseMessage.BinaryMode = serviceMap.Definition.OutputBinaryMode;

                SerializeXRoadResponse(context, requestMessage, responseMessage, result, serviceMap);
            }
        }

        protected virtual object InvokeMetaService(IServiceMap serviceMap)
        {
            return null;
        }

        protected virtual object GetServiceObject(IServiceMap serviceMap)
        {
            return null;
        }

        protected virtual void OnRequestLoaded()
        { }

        protected virtual void OnInvocationError(InvocationErrorEventArgs args)
        { }

        protected virtual void OnBeforeDeserialization(BeforeDeserializationEventArgs args)
        { }

        protected virtual void OnAfterDeserialization()
        { }

        protected virtual void OnBeforeSerialization(object result)
        { }

        protected virtual void OnAfterSerialization(object result)
        { }

        private object InvokeServiceMethod(XRoadMessage requestMessage, out IServiceMap serviceMap)
        {
            object result;
            if ((serviceMap = InvokeMetaService(requestMessage, out result)) != null)
                return result;

            serviceMap = requestMessage.GetSerializerCache().GetServiceMap(requestMessage.RootElementName);

            var serviceObject = GetServiceObject(serviceMap);

            var input = DeserializeMethodInput(requestMessage, serviceMap);

            try
            {
                result = serviceMap.Definition.MethodInfo.Invoke(serviceObject, serviceMap.HasParameters ? new[] { input } : new object[0]);
            }
            catch (Exception exception)
            {
                var e = new InvocationErrorEventArgs(exception);
                OnInvocationError(e);

                if (e.Result != null)
                    return e.Result;

                throw;
            }

            return result;
        }

        private IServiceMap InvokeMetaService(XRoadMessage requestMessage, out object result)
        {
            result = null;

            if (requestMessage.MetaServiceMap == null)
                return null;

            var serviceMap = requestMessage.MetaServiceMap;

            result = InvokeMetaService(serviceMap);

            return serviceMap;
        }

        private object DeserializeMethodInput(XRoadMessage requestMessage, IServiceMap serviceMap)
        {
            var beforeDeserializationEventArgs = new BeforeDeserializationEventArgs(serviceMap);
            OnBeforeDeserialization(beforeDeserializationEventArgs);

            requestMessage.ContentStream.Position = 0;
            var reader = XmlReader.Create(requestMessage.ContentStream, beforeDeserializationEventArgs.XmlReaderSettings);

            reader.MoveToPayload(requestMessage.RootElementName);

            var parameters = serviceMap.DeserializeRequest(reader, requestMessage);

            OnAfterDeserialization();

            return parameters;
        }

        private void SerializeXRoadResponse(HttpContext context, XRoadMessage requestMessage, XRoadMessage responseMessage, object result, IServiceMap serviceMap)
        {
            OnBeforeSerialization(result);

            requestMessage.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(requestMessage.ContentStream, new XmlReaderSettings { CloseInput = false }))
            using (var textWriter = new StreamWriter(responseMessage.ContentStream, responseMessage.ContentEncoding, 1024, true))
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

                serviceMap.SerializeResponse(writer, result, responseMessage, reader, null);

                writer.WriteEndDocument();
                writer.Flush();
            }

            responseMessage.SaveTo(context);

            OnAfterSerialization(result);
        }
    }
}

#endif