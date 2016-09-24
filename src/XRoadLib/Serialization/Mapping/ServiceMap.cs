using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    /// <summary>
    /// Provides serialization/deserialization interface for X-Road operations.
    /// </summary>
    public class ServiceMap : IServiceMap
    {
        private readonly ISerializerCache serializerCache;
        private readonly ITypeMap inputTypeMap;
        private readonly ITypeMap outputTypeMap;
        private readonly RequestValueDefinition requestValueDefinition;
        private readonly ResponseValueDefinition responseValueDefinition;

        /// <summary>
        /// Configuration settings of the operation that the ServiceMap implements.
        /// </summary>
        public OperationDefinition Definition { get; }

        /// <summary>
        /// Provides information whether this ServiceMap has any input parameters defined
        /// or has an empty request element.
        /// </summary>
        public bool HasParameters => requestValueDefinition.ParameterInfo != null;

        /// <summary>
        /// Specifies if X-Road fault is returned wrapped inside operation response element
        /// or separately as its own element.
        /// </summary>
        public bool HasXRoadFaultInResponse => responseValueDefinition.ContainsNonTechnicalFault;

        /// <summary>
        /// Initializes new ServiceMap entity using settings specified in operationDefinition.
        /// <param name="serializerCache">Provides TypeMap lookup.</param>
        /// <param name="operationDefinition">Operation which this ServiceMap represents.</param>
        /// <param name="requestValueDefinition">Defines operation request message.</param>
        /// <param name="responseValueDefinition">Defines operation response message.</param>
        /// <param name="inputTypeMap">Default TypeMap of the operation request root element.</param>
        /// <param name="outputTypeMap"> Default TypeMap of the operation response root element.</param>
        /// </summary>
        public ServiceMap(ISerializerCache serializerCache, OperationDefinition operationDefinition, RequestValueDefinition requestValueDefinition, ResponseValueDefinition responseValueDefinition, ITypeMap inputTypeMap, ITypeMap outputTypeMap)
        {
            this.serializerCache = serializerCache;
            this.inputTypeMap = inputTypeMap;
            this.outputTypeMap = outputTypeMap;
            this.requestValueDefinition = requestValueDefinition;
            this.responseValueDefinition = responseValueDefinition;

            Definition = operationDefinition;
        }

        /// <summary>
        /// Deserializes X-Road message protocol requests according to operation definitions.
        /// </summary>
        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            var requestName = message.Protocol.RequestPartNameInRequest;

            if (!requestValueDefinition.MergeContent && !reader.MoveToElement(3, requestName))
                throw XRoadException.InvalidQuery($"Päringus puudub X-tee `{requestName}` element.");

            if (requestValueDefinition.ParameterInfo != null)
                return DeserializeValue(reader, inputTypeMap, message.RequestNode, requestValueDefinition, message);

            return null;
        }

        /// <summary>
        /// Deserializes X-Road message protocol responses according to operation definitions.
        /// </summary>
        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            var requestName = message.Protocol.RequestPartNameInResponse;
            var responseName = message.Protocol.ResponsePartNameInResponse;

            if (!Definition.ProhibitRequestPartInResponse)
            {
                if (!reader.MoveToElement(3, requestName))
                    throw XRoadException.InvalidQuery($"Expected payload element `{requestName}` was not found in SOAP message.");
                reader.Read();
            }

            var hasResponseElement = reader.MoveToElement(3);

            if (hasResponseElement && !HasXRoadFaultInResponse && reader.LocalName == responseValueDefinition.FaultName)
                return reader.ReadXRoadFault(4);

            if (!hasResponseElement || reader.LocalName != responseName)
                throw XRoadException.InvalidQuery($"Expected payload element `{responseName}` in SOAP message, but `{reader.LocalName}` was found instead.");

            var hasWrapperElement = HasWrapperResultElement(message);
            if (hasWrapperElement && !reader.MoveToElement(4, responseValueDefinition.Name.LocalName, responseValueDefinition.Name.NamespaceName))
                throw XRoadException.InvalidQuery($"Expected result wrapper element `{responseValueDefinition.Name}` was not found in SOAP message.");

            return DeserializeValue(reader, outputTypeMap, message.ResponseNode, responseValueDefinition, message);
        }

        private object DeserializeValue(XmlReader reader, ITypeMap typeMap, IXmlTemplateNode templateNode, IContentDefinition contentDefinition, XRoadMessage message)
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
            if (!contentDefinition.IgnoreExplicitType)
                concreteTypeMap = (typeMap.Definition.IsInheritable ? serializerCache.GetTypeMapFromXsiType(reader) : null) ?? typeMap;

            return concreteTypeMap.Deserialize(reader, templateNode, contentDefinition, message);
        }

        /// <summary>
        /// Serializes X-Road message protocol requests according to operation definitions.
        /// </summary>
        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace = null)
        {
            var ns = string.IsNullOrEmpty(requestNamespace) ? Definition.Name.NamespaceName : requestNamespace;
            var addPrefix = writer.LookupPrefix(ns) == null;

            if (addPrefix) writer.WriteStartElement(PrefixConstants.TARGET, Definition.Name.LocalName, ns);
            else writer.WriteStartElement(Definition.Name.LocalName, ns);

            if (!requestValueDefinition.MergeContent)
                writer.WriteStartElement(message.Protocol.RequestPartNameInRequest);

            if (requestValueDefinition.ParameterInfo != null)
                SerializeValue(writer, value, inputTypeMap, message.RequestNode, message, requestValueDefinition);

            if (!requestValueDefinition.MergeContent)
                writer.WriteEndElement();

            writer.WriteEndElement();
        }

        /// <summary>
        /// Serializes X-Road message protocol responses according to operation definitions.
        /// </summary>
        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization)
        {
            var containsRequest = requestReader.MoveToElement(2, Definition.Name.LocalName, Definition.Name.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);
            else writer.WriteStartElement($"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);

            var fault = value as IXRoadFault;
            var namespaceInContext = requestReader.NamespaceURI;

            if (containsRequest && !Definition.ProhibitRequestPartInResponse)
                CopyRequestToResponse(writer, requestReader, message, out namespaceInContext);

            if (!HasXRoadFaultInResponse && fault != null)
            {
                writer.WriteStartElement(responseValueDefinition.FaultName);
                SerializeFault(writer, fault, message.Protocol);
                writer.WriteEndElement();
            }
            else if (outputTypeMap != null)
            {
                if (Equals(namespaceInContext, ""))
                    writer.WriteStartElement(message.Protocol.ResponsePartNameInResponse);
                else writer.WriteStartElement(message.Protocol.ResponsePartNameInResponse, "");

                if (fault != null)
                    SerializeFault(writer, fault, message.Protocol);
                else if (outputTypeMap != null)
                {
                    var addWrapperElement = HasWrapperResultElement(message);

                    if (addWrapperElement)
                        writer.WriteStartElement(responseValueDefinition.Name.LocalName, responseValueDefinition.Name.NamespaceName);

                    SerializeValue(writer, value, outputTypeMap, message.ResponseNode, message, responseValueDefinition);

                    if (addWrapperElement)
                        writer.WriteEndElement();
                }

                writer.WriteEndElement();

                customSerialization?.OnContentComplete(writer);
            }

            writer.WriteEndElement();
        }

        private void SerializeValue(XmlWriter writer, object value, ITypeMap typeMap, IXmlTemplateNode templateNode, XRoadMessage message, IContentDefinition contentDefinition)
        {
            if (value == null)
            {
                writer.WriteNilAttribute();
                return;
            }

            var concreteTypeMap = typeMap.Definition.IsInheritable ? serializerCache.GetTypeMap(value.GetType()) : typeMap;

            concreteTypeMap.Serialize(writer, templateNode, value, contentDefinition, message);
        }

        private bool HasWrapperResultElement(XRoadMessage message)
        {
            return !responseValueDefinition.MergeContent
                   && responseValueDefinition.XRoadFaultPresentation != XRoadFaultPresentation.Implicit
                   && HasXRoadFaultInResponse;
        }

        private static void SerializeFault(XmlWriter writer, IXRoadFault fault, XRoadProtocol protocol)
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

        private static void CopyRequestToResponse(XmlWriter writer, XmlReader reader, XRoadMessage message, out string namespaceInContext)
        {
            namespaceInContext = reader.NamespaceURI;

            writer.WriteAttributes(reader, true);

            if (!reader.MoveToElement(3) || !reader.IsCurrentElement(3, message.Protocol.RequestPartNameInRequest))
                return;

            if (message.Protocol.RequestPartNameInRequest != message.Protocol.RequestPartNameInResponse)
            {
                writer.WriteStartElement(message.Protocol.RequestPartNameInResponse);
                writer.WriteAttributes(reader, true);

                while (reader.MoveToElement(4))
                    writer.WriteNode(reader, true);

                writer.WriteEndElement();
            }
            else writer.WriteNode(reader, true);
        }
    }
}