using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Handler.Events;
using XRoadLib.Protocols;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Handler
{
    public abstract class ServiceRequestHandlerBase : ServiceHandlerBase
    {
        private readonly ICollection<IProtocol> supportedProtocols;

        public string StoragePath { get; set; }
        public ICustomSerialization CustomSerialization { get; set; }

        protected ServiceRequestHandlerBase(IEnumerable<IProtocol> supportedProtocols)
        {
            if (supportedProtocols == null)
                throw new ArgumentNullException(nameof(supportedProtocols));
            this.supportedProtocols = new List<IProtocol>(supportedProtocols);
        }

        protected abstract object InvokeMetaService(MetaServiceName metaServiceName);

        protected abstract object GetServiceObject(string operationName);

        protected virtual void OnRequestLoaded()
        { }

        protected virtual void OnInvocationError(InvocationErrorEventArgs args)
        { }

        protected virtual void OnBeforeDeserialization(BeforeDeserializationEventArgs args)
        { }

        protected virtual void OnAfterDeserialization()
        { }

        protected virtual void OnBeforeSerialization(object result, SerializationContext context)
        { }

        protected virtual void OnAfterSerialization(object result)
        { }

        protected override void HandleRequest(HttpContext httpContext)
        {
            if (httpContext.Request.InputStream.Length == 0)
                throw XRoadException.InvalidQuery("Empty request content");

            requestMessage.LoadRequest(httpContext, StoragePath.GetValueOrDefault(Path.GetTempPath()), supportedProtocols);
            if (requestMessage.Protocol == null)
                throw XRoadException.InvalidQuery("Could not detect X-Road message protocol version from request message. Adapter supports following protocol versions: {0}.", string.Join(", ", supportedProtocols.Select(x => $@"""{x.Name}""")));

            responseMessage.Copy(requestMessage);

            OnRequestLoaded();

            IServiceMap serviceMap;
            var result = InvokeServiceMethod(requestMessage.Protocol.SerializerCache, out serviceMap);

            if (serviceMap.HasMultipartResponse)
                responseMessage.IsMultipart = true;

            SerializeXRoadResponse(httpContext, result, serviceMap);
        }

        private object InvokeServiceMethod(ISerializerCache serializerCache, out IServiceMap serviceMap)
        {
            object result;
            if ((serviceMap = InvokeMetaService(serializerCache, out result)) != null)
                return result;

            var operationName = requestMessage.Header?.Service?.ServiceCode ?? requestMessage.RootElementName?.LocalName;
            var serviceObject = GetServiceObject(operationName);

            serviceMap = serializerCache.GetServiceMap(requestMessage.RootElementName, (requestMessage.Header?.Service?.Version).GetValueOrDefault(1u));

            var parameters = DeserializeMethodParameters(serviceMap);

            try
            {
                result = InvokeDataMethod(serviceObject, serviceMap.MethodInfo, parameters);
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

        private IServiceMap InvokeMetaService(ISerializerCache serializerCache, out object result)
        {
            result = null;

            var metaServiceName = requestMessage.GetMetaServiceName();
            if (metaServiceName == MetaServiceName.None)
                return null;

            var serviceMap = serializerCache.GetServiceMap(requestMessage.RootElementName, 1u);

            result = InvokeMetaService(metaServiceName);

            return serviceMap;
        }

        private IDictionary<string, object> DeserializeMethodParameters(IServiceMap serviceMap)
        {
            var context = requestMessage.CreateContext();

            var beforeDeserializationEventArgs = new BeforeDeserializationEventArgs(context, serviceMap);
            OnBeforeDeserialization(beforeDeserializationEventArgs);

            requestMessage.ContentStream.Position = 0;
            var reader = XmlReader.Create(requestMessage.ContentStream, beforeDeserializationEventArgs.XmlReaderSettings);

            reader.MoveToPayload(requestMessage.RootElementName);

            var parameters = serviceMap.DeserializeRequest(reader, context);

            OnAfterDeserialization();

            return parameters;
        }

        private static object InvokeDataMethod(object targetObject, MethodBase methodInfo, IDictionary<string, object> parameters)
        {
            var parameterInfos = methodInfo.GetParameters();
            var methodParams = new object[parameterInfos.Length];

            for (var i = 0; i < parameterInfos.Length; i++)
                parameters.TryGetValue(parameterInfos[i].Name, out methodParams[i]);

            return methodInfo.Invoke(targetObject, methodParams);
        }

        private void SerializeXRoadResponse(HttpContext httpContext, object result, IServiceMap serviceMap)
        {
            var context = responseMessage.CreateContext();
            OnBeforeSerialization(result, context);

            requestMessage.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(requestMessage.ContentStream, new XmlReaderSettings { CloseInput = false }))
            {
                var writer = new XmlTextWriter(responseMessage.ContentStream, responseMessage.ContentEncoding);

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

                serviceMap.SerializeResponse(writer, result, context, reader, CustomSerialization);

                writer.WriteEndDocument();
                writer.Flush();
            }

            responseMessage.SaveTo(httpContext);

            OnAfterSerialization(result);
        }
    }
}