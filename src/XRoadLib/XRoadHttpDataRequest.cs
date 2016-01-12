using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Xml;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib
{
    public delegate void EventHandler(XRoadHttpDataRequest sender, EventArgs e);

    public class XRoadHttpDataRequest : IDisposable
    {
        private const string RESPONSE_CONTENT_TYPE = "text/xml; charset=utf-8";

        private readonly HttpContext httpContext;
        private readonly IProtocolSerializerCache protocolSerializerCache;
        private readonly IServiceRunner serviceRunner;

        public XRoadMessage RequestMessage { get; private set; }
        public XRoadMessage ResponseMessage { get; private set; }

        public event ExceptionOccuredEventHandler ExceptionOccured;
        public event EventHandler RequestLoaded;
        public event InvocationErrorHandler InvocationError;
        public event BeforeDeserializationEventHandler BeforeDeserialization;
        public event EventHandler AfterDeserialization;
        public event BeforeSerializationEventHandler BeforeSerialization;
        public event AfterSerializationEventHandler AfterSerialization;

        public string StoragePath { get; set; }
        public ICustomSerialization CustomSerialization { get; set; }

        public XRoadHttpDataRequest(HttpContext httpContext, IProtocolSerializerCache protocolSerializerCache, IServiceRunner serviceRunner)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));
            this.httpContext = httpContext;

            if (protocolSerializerCache == null)
                throw new ArgumentNullException(nameof(protocolSerializerCache));
            this.protocolSerializerCache = protocolSerializerCache;

            if (serviceRunner == null)
                throw new ArgumentNullException(nameof(serviceRunner));
            this.serviceRunner = serviceRunner;

            RequestMessage = new XRoadMessage();
            ResponseMessage = new XRoadMessage(new MemoryStream());
        }

        public void Process()
        {
            httpContext.Request.InputStream.Position = 0;
            httpContext.Response.ContentType = RESPONSE_CONTENT_TYPE;

            try
            {
                ProcessRequest();
            }
            catch (Exception exception)
            {
                var e = new ExceptionOccuredEventArgs(exception);
                ExceptionOccured?.Invoke(this, e);

                using (var writer = new XmlTextWriter(httpContext.Response.OutputStream, httpContext.Response.ContentEncoding))
                    SoapMessageHelper.SerializeSoapFaultResponse(writer, e.Code, e.Message, e.Actor, e.Detail, exception);
            }
        }

        private void ProcessRequest()
        {
            if (httpContext.Request.InputStream.Length == 0)
                throw XRoadException.InvalidQuery("Empty request content");

            RequestMessage.LoadRequest(httpContext, StoragePath.GetValueOrDefault(Path.GetTempPath()));
            ResponseMessage.Copy(RequestMessage);

            var serializerCache = protocolSerializerCache.GetSerializerCache(RequestMessage.Protocol);

            RequestLoaded?.Invoke(this, new EventArgs());

            IServiceMap serviceMap;
            var result = InvokeServiceMethod(serializerCache, out serviceMap);

            if (serviceMap.HasMultipartResponse)
                ResponseMessage.IsMultipart = true;

            SerializeXRoadResponse(result, serviceMap);
        }

        private object InvokeServiceMethod(ISerializerCache serializerCache, out IServiceMap serviceMap)
        {
            object result;
            if ((serviceMap = InvokeMetaService(serializerCache, out result)) != null)
                return result;

            var operationName = RequestMessage.Header?.Nimi?.Method ?? RequestMessage.RootElementName?.Name;
            var dataService = serviceRunner.GetDataService(operationName);

            serviceMap = serializerCache.GetServiceMap(RequestMessage.RootElementName, (RequestMessage.Header?.Nimi?.Version).GetValueOrDefault(1u), dataService.Item2);

            var parameters = DeserializeMethodParameters(serviceMap, dataService.Item2);

            try
            {
                result = InvokeDataMethod(dataService.Item1, dataService.Item2, parameters);
            }
            catch (Exception exception)
            {
                var e = new InvocationErrorEventArgs(exception);
                InvocationError?.Invoke(this, e);

                if (e.Result != null)
                    return e.Result;

                throw;
            }

            return result;
        }

        private IServiceMap InvokeMetaService(ISerializerCache serializerCache, out object result)
        {
            result = null;

            var metaServiceName = RequestMessage.GetMetaServiceName();
            if (metaServiceName == MetaServiceName.None)
                return null;

            var serviceMap = serializerCache.GetServiceMap(RequestMessage.RootElementName, 1u, null);

            result = serviceRunner.InvokeMetaService(metaServiceName);

            return serviceMap;
        }

        private IDictionary<string, object> DeserializeMethodParameters(IServiceMap serviceMap, MethodInfo methodInfo)
        {
            var context = RequestMessage.CreateContext();

            var beforeDeserializationEventArgs = new BeforeDeserializationEventArgs(context, methodInfo);
            BeforeDeserialization?.Invoke(this, beforeDeserializationEventArgs);

            RequestMessage.ContentStream.Position = 0;
            var reader = XmlReader.Create(RequestMessage.ContentStream, beforeDeserializationEventArgs.XmlReaderSettings);

            if (!reader.MoveToElement(0, "Envelope", NamespaceHelper.SOAP_ENV))
                throw XRoadException.InvalidQuery("Element `{0}:Envelope` is missing from request content.", NamespaceHelper.SOAP);
            if (!reader.MoveToElement(1, "Body", NamespaceHelper.SOAP_ENV))
                throw XRoadException.InvalidQuery("Element `{0}:Body` is missing from request content.", NamespaceHelper.SOAP);
            if (!reader.MoveToElement(2, RequestMessage.RootElementName.Name, RequestMessage.RootElementName.Namespace))
                throw XRoadException.InvalidQuery("Payload is missing from request content.");

            var parameters = serviceMap.DeserializeRequest(reader, context);

            AfterDeserialization?.Invoke(this, new EventArgs());

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

        private void SerializeXRoadResponse(object result, IServiceMap serviceMap)
        {
            var context = ResponseMessage.CreateContext();
            BeforeSerialization?.Invoke(this, new BeforeSerializationEventArgs(result, context));

            RequestMessage.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(RequestMessage.ContentStream, new XmlReaderSettings { CloseInput = false }))
            {
                var writer = new XmlTextWriter(ResponseMessage.ContentStream, ResponseMessage.ContentEncoding);

                writer.WriteStartDocument();

                if (reader.MoveToElement(0) && reader.IsCurrentElement(0, "Envelope", NamespaceHelper.SOAP_ENV))
                {
                    writer.WriteStartElement(reader.Prefix, "Envelope", NamespaceHelper.SOAP_ENV);
                    writer.WriteAttributes(reader, true);
                }
                else
                {
                    writer.WriteStartElement(PrefixHelper.SOAP_ENV, "Envelope", NamespaceHelper.SOAP_ENV);
                    writer.WriteAttributeString("xmlns", PrefixHelper.SOAP_ENV, NamespaceHelper.XMLNS, NamespaceHelper.SOAP_ENV);
                }

                if (reader.MoveToElement(1) && reader.IsCurrentElement(1, "Header", NamespaceHelper.SOAP_ENV))
                    writer.WriteNode(reader, true);

                writer.WriteStartElement("Body", NamespaceHelper.SOAP_ENV);
                if (reader.IsCurrentElement(1, "Body", NamespaceHelper.SOAP_ENV) || reader.MoveToElement(1, "Body", NamespaceHelper.SOAP_ENV))
                    writer.WriteAttributes(reader, true);

                serviceMap.SerializeResponse(writer, result, context, reader, CustomSerialization);

                writer.WriteEndDocument();
                writer.Flush();
            }

            ResponseMessage.SaveTo(httpContext);

            AfterSerialization?.Invoke(this, new AfterSerializationEventArgs(result));
        }

        public void Dispose()
        {
            RequestMessage?.Dispose();
            RequestMessage = null;

            ResponseMessage?.Dispose();
            ResponseMessage = null;
        }
    }
}