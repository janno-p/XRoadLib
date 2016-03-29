using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Serialization.Mapping
{
    public class ListMethodsServiceMap : IServiceMap
    {
        public OperationDefinition Definition { get; }

        public bool HasParameters => false;

        public ListMethodsServiceMap(XName operationName)
        {
            var methodInfo = typeof(Implementation).GetMethod("Execute");

            Definition = new OperationDefinition(operationName, null, methodInfo);
        }

        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            throw new System.NotImplementedException();
        }

        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message)
        {
            throw new System.NotImplementedException();
        }

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
                writer.WriteCDataEscape(methodName);
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