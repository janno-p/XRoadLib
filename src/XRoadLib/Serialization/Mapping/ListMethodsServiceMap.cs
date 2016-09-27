using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Styles;

#if !NET40
using System.Reflection;
#endif

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
        public OperationDefinition OperationDefinition { get; }

        /// <summary>
        /// Response element specification of the X-Road operation.
        /// </summary>
        public RequestValueDefinition RequestValueDefinition { get; }

        /// <summary>
        /// Response element specification of the X-Road operation.
        /// </summary>
        public ResponseValueDefinition ResponseValueDefinition { get; }

        /// <summary>
        /// Initialize new `listMethods` service map.
        /// </summary>
        public ListMethodsServiceMap(XName operationName)
        {
            var methodInfo = typeof(Implementation).GetTypeInfo().GetMethod("Execute");

            OperationDefinition = new OperationDefinition(operationName, null, methodInfo);
            RequestValueDefinition = new RequestValueDefinition(OperationDefinition);
            ResponseValueDefinition = new ResponseValueDefinition(OperationDefinition)
            {
                ContainsNonTechnicalFault = true,
                ResponseElementName = operationName.NamespaceName == NamespaceConstants.XTEE ? "keha" : "response"
            };
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
            var containsRequest = requestReader.MoveToElement(2, OperationDefinition.Name.LocalName, OperationDefinition.Name.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{OperationDefinition.Name.LocalName}Response", OperationDefinition.Name.NamespaceName);
            else writer.WriteStartElement($"{OperationDefinition.Name.LocalName}Response", OperationDefinition.Name.NamespaceName);

            var namespaceInContext = requestReader.NamespaceURI;

            var responsePartName = OperationDefinition.Name.NamespaceName.Equals(NamespaceConstants.XTEE) ? "keha" : "response";
            var style = OperationDefinition.Name.NamespaceName.Equals(NamespaceConstants.XTEE) ? (Style)new RpcEncodedStyle() : new DocLiteralStyle();

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
                writer.WriteString(methodName);
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