using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    /// <inheritdoc />
    public class ServiceMap : IServiceMap
    {
        private readonly ISerializerCache serializerCache;
        private readonly ITypeMap inputTypeMap;
        private readonly ITypeMap outputTypeMap;

        /// <inheritdoc />
        public OperationDefinition OperationDefinition { get; }

        /// <inheritdoc />
        public RequestDefinition RequestDefinition { get; }

        /// <inheritdoc />
        public ResponseDefinition ResponseDefinition { get; }

        private bool HasWrapperResultElement =>
            !ResponseDefinition.Content.MergeContent
            && ResponseDefinition.XRoadFaultPresentation != XRoadFaultPresentation.Implicit
            && ResponseDefinition.ContainsNonTechnicalFault;

        /// <summary>
        /// Initializes new ServiceMap entity using settings specified in operationDefinition.
        /// <param name="serializerCache">Provides TypeMap lookup.</param>
        /// <param name="operationDefinition">Operation which this ServiceMap represents.</param>
        /// <param name="requestDefinition">Defines operation request message.</param>
        /// <param name="responseDefinition">Defines operation response message.</param>
        /// <param name="inputTypeMap">Default TypeMap of the operation request root element.</param>
        /// <param name="outputTypeMap"> Default TypeMap of the operation response root element.</param>
        /// </summary>
        public ServiceMap(ISerializerCache serializerCache, OperationDefinition operationDefinition, RequestDefinition requestDefinition, ResponseDefinition responseDefinition, ITypeMap inputTypeMap, ITypeMap outputTypeMap)
        {
            this.serializerCache = serializerCache;

            RequestDefinition = requestDefinition;
            ResponseDefinition = responseDefinition;
            OperationDefinition = operationDefinition;

            this.inputTypeMap = inputTypeMap is IContentTypeMap inputContentTypeMap && requestDefinition.Content.UseXop
                ? inputContentTypeMap.GetOptimizedContentTypeMap()
                : inputTypeMap;

            this.outputTypeMap = outputTypeMap is IContentTypeMap outputContentTypeMap && responseDefinition.Content.UseXop
                ? outputContentTypeMap.GetOptimizedContentTypeMap()
                : outputTypeMap;
        }

        /// <inheritdoc />
        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            var requestName = RequestDefinition.RequestElementName;

            if (!RequestDefinition.Content.MergeContent && !reader.MoveToElement(3, requestName))
                throw XRoadException.InvalidQuery($"Päringus puudub X-tee `{requestName}` element.");

            return RequestDefinition.ParameterInfo != null
                ? ProcessRequestValue(DeserializeValue(reader, inputTypeMap, message.RequestNode, RequestDefinition.Content, message))
                : null;
        }

        /// <inheritdoc />
        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            var requestName = ResponseDefinition.RequestElementName;
            var responseName = ResponseDefinition.ResponseElementName;

            if (OperationDefinition.CopyRequestPartToResponse)
            {
                if (!reader.MoveToElement(3, requestName))
                    throw XRoadException.InvalidQuery($"Expected payload element `{requestName}` was not found in SOAP message.");
                reader.Read();
            }

            var hasResponseElement = reader.MoveToElement(3);

            if (hasResponseElement && !ResponseDefinition.ContainsNonTechnicalFault && reader.LocalName == ResponseDefinition.FaultName)
                return reader.ReadXRoadFault(4);

            if (!hasResponseElement || reader.LocalName != responseName)
                throw XRoadException.InvalidQuery($"Expected payload element `{responseName}` in SOAP message, but `{reader.LocalName}` was found instead.");

            var hasWrapperElement = HasWrapperResultElement;
            if (hasWrapperElement && !reader.MoveToElement(4, ResponseDefinition.Content.Name.LocalName, ResponseDefinition.Content.Name.NamespaceName))
                throw XRoadException.InvalidQuery($"Expected result wrapper element `{ResponseDefinition.Content.Name}` was not found in SOAP message.");

            return ProcessResponseValue(DeserializeValue(reader, outputTypeMap, message.ResponseNode, ResponseDefinition.Content, message));
        }

        private object DeserializeValue(XmlReader reader, ITypeMap typeMap, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            if (reader.IsNilElement())
            {
                reader.ReadToEndElement();
                return null;
            }

            string typeAttribute;
            if (typeMap.Definition.IsAnonymous && !(typeMap is IArrayTypeMap) && (typeAttribute = reader.GetAttribute("type", NamespaceConstants.XSI)) != null)
                throw XRoadException.InvalidQuery($"Expected anonymous type, but `{typeAttribute}` was given.");

            var concreteTypeMap = typeMap;
            if (!content.IgnoreExplicitType)
                concreteTypeMap = (typeMap.Definition.IsInheritable ? serializerCache.GetTypeMapFromXsiType(reader) : null) ?? typeMap;

            return concreteTypeMap.Deserialize(reader, templateNode, content, message);
        }

        /// <inheritdoc />
        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace = null)
        {
            var ns = string.IsNullOrEmpty(requestNamespace) ? OperationDefinition.Name.NamespaceName : requestNamespace;
            var addPrefix = writer.LookupPrefix(ns) == null;

            if (addPrefix) writer.WriteStartElement(PrefixConstants.TARGET, OperationDefinition.Name.LocalName, ns);
            else writer.WriteStartElement(OperationDefinition.Name.LocalName, ns);

            if (!RequestDefinition.Content.MergeContent)
                writer.WriteStartElement(RequestDefinition.RequestElementName);

            if (RequestDefinition.ParameterInfo != null)
                SerializeValue(writer, PrepareRequestValue(value), inputTypeMap, message.RequestNode, message, RequestDefinition.Content);

            if (!RequestDefinition.Content.MergeContent)
                writer.WriteEndElement();

            writer.WriteEndElement();
        }

        /// <summary>
        /// Serializes X-Road message protocol responses according to operation definitions.
        /// </summary>
        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization)
        {
            var containsRequest = requestReader.MoveToElement(2, OperationDefinition.Name.LocalName, OperationDefinition.Name.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{OperationDefinition.Name.LocalName}Response", OperationDefinition.Name.NamespaceName);
            else writer.WriteStartElement($"{OperationDefinition.Name.LocalName}Response", OperationDefinition.Name.NamespaceName);

            var fault = value as IXRoadFault;
            var namespaceInContext = requestReader.NamespaceURI;

            if (containsRequest && OperationDefinition.CopyRequestPartToResponse)
                CopyRequestToResponse(writer, requestReader);

            if (!ResponseDefinition.ContainsNonTechnicalFault && fault != null)
            {
                writer.WriteStartElement(ResponseDefinition.FaultName);
                SerializeFault(writer, fault, message.Protocol);
                writer.WriteEndElement();
            }
            else if (outputTypeMap != null)
            {
                if (Equals(namespaceInContext, ""))
                    writer.WriteStartElement(ResponseDefinition.ResponseElementName);
                else writer.WriteStartElement(ResponseDefinition.ResponseElementName, "");

                if (fault != null)
                    SerializeFault(writer, fault, message.Protocol);
                else if (outputTypeMap != null)
                {
                    var addWrapperElement = HasWrapperResultElement;
                    if (addWrapperElement)
                        writer.WriteStartElement(ResponseDefinition.Content.Name.LocalName, ResponseDefinition.Content.Name.NamespaceName);

                    SerializeValue(writer, PrepareResponseValue(value), outputTypeMap, message.ResponseNode, message, ResponseDefinition.Content);

                    if (addWrapperElement)
                        writer.WriteEndElement();
                }

                writer.WriteEndElement();

                customSerialization?.OnContentComplete(writer);
            }

            writer.WriteEndElement();
        }

        protected virtual object PrepareRequestValue(object value) => value;
        protected virtual object PrepareResponseValue(object value) => value;
        protected virtual object ProcessRequestValue(object value) => value;
        protected virtual object ProcessResponseValue(object value) => value;

        private void SerializeValue(XmlWriter writer, object value, ITypeMap typeMap, IXmlTemplateNode templateNode, XRoadMessage message, ContentDefinition content)
        {
            if (value == null)
            {
                writer.WriteNilAttribute();
                return;
            }

            var concreteTypeMap = typeMap.Definition.IsInheritable ? serializerCache.GetTypeMap(value.GetType()) : typeMap;

            concreteTypeMap.Serialize(writer, templateNode, value, content, message);
        }

        private static void SerializeFault(XmlWriter writer, IXRoadFault fault, IXRoadProtocol protocol)
        {
            writer.WriteStartElement("faultCode");
            protocol.Style.WriteExplicitType(writer, XName.Get("string", NamespaceConstants.XSD));
            writer.WriteValue(fault.FaultCode);
            writer.WriteEndElement();

            writer.WriteStartElement("faultString");
            protocol.Style.WriteExplicitType(writer, XName.Get("string", NamespaceConstants.XSD));
            writer.WriteValue(fault.FaultString);
            writer.WriteEndElement();
        }

        private void CopyRequestToResponse(XmlWriter writer, XmlReader reader)
        {
            writer.WriteAttributes(reader, true);

            if (!reader.MoveToElement(3) || !reader.IsCurrentElement(3, RequestDefinition.RequestElementName))
                return;

            if (RequestDefinition.RequestElementName != ResponseDefinition.RequestElementName)
            {
                writer.WriteStartElement(ResponseDefinition.RequestElementName);
                writer.WriteAttributes(reader, true);

                while (reader.MoveToElement(4))
                    writer.WriteNode(reader, true);

                writer.WriteEndElement();
            }
            else writer.WriteNode(reader, true);
        }
    }
}