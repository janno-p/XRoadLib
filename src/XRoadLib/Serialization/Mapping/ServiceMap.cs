using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Schema;

namespace XRoadLib.Serialization.Mapping
{
    public class ServiceMap : IServiceMap
    {
        private readonly ISerializerCache serializerCache;
        private readonly ITypeMap inputTypeMap;
        private readonly ITypeMap outputTypeMap;
        private readonly ResponseValueDefinition responseValueDefinition;

        public OperationDefinition Definition { get; }

        public ServiceMap(ISerializerCache serializerCache, OperationDefinition operationDefinition, ITypeMap inputTypeMap, ITypeMap outputTypeMap, ResponseValueDefinition responseValueDefinition)
        {
            this.serializerCache = serializerCache;
            this.inputTypeMap = inputTypeMap;
            this.outputTypeMap = outputTypeMap;
            this.responseValueDefinition = responseValueDefinition;

            Definition = operationDefinition;
        }

        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            var requestName = message.Protocol.RequestPartNameInRequest;

            if (!reader.MoveToElement(3, requestName))
                throw XRoadException.InvalidQuery($"Päringus puudub X-tee `{requestName}` element.");

            return inputTypeMap.Deserialize(reader, message.RequestNode, message, true);
        }

        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            var responseName = message.Protocol.ResponsePartNameInResponse;

            if (!reader.MoveToElement(3, responseName))
                throw XRoadException.InvalidQuery($"Expected payload element `{responseName}` was not found in SOAP message.");

            if (reader.IsNilElement())
            {
                reader.ReadToEndElement();
                return null;
            }

            string typeAttribute;
            if (outputTypeMap.Definition.IsAnonymous && !(outputTypeMap is IArrayTypeMap) && (typeAttribute = reader.GetAttribute("type", NamespaceConstants.XSI)) != null)
                throw XRoadException.InvalidQuery("Expected anonymous type, but `{0}` was given.", typeAttribute);

            var concreteTypeMap = (outputTypeMap.Definition.IsInheritable ? serializerCache.GetTypeMapFromXsiType(reader) : null) ?? outputTypeMap;

            return concreteTypeMap.Deserialize(reader, message.ResponseNode, message);
        }

        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message)
        {
            writer.WriteStartElement(Definition.Name.LocalName, Definition.Name.NamespaceName);
            writer.WriteStartElement(message.Protocol.RequestPartNameInRequest);

            inputTypeMap.Serialize(writer, message.RequestNode, value, inputTypeMap.Definition.Type, message);

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization)
        {
            var containsRequest = requestReader.MoveToElement(2, Definition.Name.LocalName, Definition.Name.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);
            else writer.WriteStartElement($"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);

            var namespaceInContext = requestReader.NamespaceURI;
            if (containsRequest && !Definition.ProhibitRequestPartInResponse)
                CopyRequestToResponse(writer, requestReader, message, out namespaceInContext);

            var fault = value as IXRoadFault;
            if (Definition.ProhibitRequestPartInResponse && fault != null)
                SerializeFault(writer, fault, message.Protocol);
            else if (outputTypeMap != null)
            {
                if (Equals(namespaceInContext, ""))
                    writer.WriteStartElement(message.Protocol.ResponsePartNameInResponse);
                else writer.WriteStartElement(message.Protocol.ResponsePartNameInResponse, "");

                if (fault != null)
                    SerializeFault(writer, fault, message.Protocol);
                else if (outputTypeMap != null)
                {
                    if (responseValueDefinition.HasExplicitXRoadFault)
                        writer.WriteStartElement(responseValueDefinition.Name.LocalName, responseValueDefinition.Name.NamespaceName);

                    if (value == null)
                        writer.WriteNilAttribute();
                    else
                    {
                        var concreteTypeMap = outputTypeMap.Definition.IsInheritable ? serializerCache.GetTypeMap(value.GetType()) : outputTypeMap;

                        concreteTypeMap.Serialize(writer, message.ResponseNode, value, outputTypeMap.Definition.Type, message);
                    }

                    if (responseValueDefinition.HasExplicitXRoadFault)
                        writer.WriteEndElement();
                }

                writer.WriteEndElement();

                customSerialization?.OnContentComplete(writer);
            }

            writer.WriteEndElement();
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