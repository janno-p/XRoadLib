using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Styles;

namespace XRoadLib.Serialization.Mapping
{
    /// <inheritdoc />
    public class ListMethodsServiceMap : IServiceMap
    {
        /// <inheritdoc />
        public OperationDefinition OperationDefinition { get; }

        /// <inheritdoc />
        public RequestDefinition RequestDefinition { get; }

        /// <inheritdoc />
        public ResponseDefinition ResponseDefinition { get; }

        /// <summary>
        /// Initialize new `listMethods` service map.
        /// </summary>
        public ListMethodsServiceMap(XName operationName)
        {
            var methodInfo = typeof(Implementation).GetTypeInfo().GetMethod("Execute");

            OperationDefinition = new OperationDefinition(operationName, null, methodInfo);
            RequestDefinition = new RequestDefinition(OperationDefinition, _ => false);
            ResponseDefinition = new ResponseDefinition(OperationDefinition, _ => false)
            {
                ContainsNonTechnicalFault = true,
                ResponseElementName = operationName.NamespaceName == NamespaceConstants.XTEE ? "keha" : "response"
            };
        }

        /// <inheritdoc />
        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization = null)
        {
            var containsRequest = requestReader.MoveToElement(2, OperationDefinition.Name);

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
            // ReSharper disable once UnusedMember.Local
            public string[] Execute()
            {
                return null;
            }
        }
    }
}