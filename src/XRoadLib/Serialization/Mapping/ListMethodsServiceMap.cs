using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Serialization.Mapping
{
    /// <summary>
    /// Virtual service map which implements `listMethods` request.
    /// </summary>
    public class ListMethodsServiceMap : IServiceMap
    {
        /// <summary>
        /// Operation specification.
        /// </summary>
        public OperationDefinition Definition { get; }

        /// <summary>
        /// Any parameters for the operation?
        /// </summary>
        public bool HasParameters => false;

        /// <summary>
        /// Initialize new `listMethods` service map.
        /// </summary>
        public ListMethodsServiceMap(XName operationName)
        {
            var methodInfo = typeof(Implementation).GetTypeInfo().GetMethod("Execute");

            Definition = new OperationDefinition(operationName, null, methodInfo);
        }

        /// <summary>
        /// Deserializes `listMethods` request.
        /// </summary>
        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Deserializes `listMethods` response.
        /// </summary>
        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Serializes `listMethods` request.
        /// </summary>
        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Serializes `listMethods` response.
        /// </summary>
        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization = null)
        {
            var containsRequest = requestReader.MoveToElement(2, Definition.Name.LocalName, Definition.Name.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);
            else writer.WriteStartElement($"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);

            var namespaceInContext = requestReader.NamespaceURI;

            var responsePartName = Definition.Name.NamespaceName.Equals(NamespaceConstants.XTEE) ? "keha" : "response";
            var style = Definition.Name.NamespaceName.Equals(NamespaceConstants.XTEE) ? (Style)new RpcEncodedStyle() : new DocLiteralStyle();

            if (Equals(namespaceInContext, ""))
                writer.WriteStartElement(responsePartName);
            else writer.WriteStartElement(responsePartName, "");

            var typeName = XName.Get("string", NamespaceConstants.XSD);
            var methodNames = value as ICollection<string> ?? new string[0];

            style.WriteExplicitArrayType(writer, typeName, methodNames.Count);

            foreach (var methodName in methodNames)
            {
                writer.WriteStartElement("item");
                style.WriteExplicitType(writer, typeName);
                writer.WriteStringWithMode(methodName, message.Protocol.StringSerializationMode);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private class Implementation
        {
            public string[] Execute()
            {
                return null;
            }
        }
    }
}