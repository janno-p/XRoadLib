using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;

#if !NET40
using System.Reflection;
#endif

namespace XRoadLib.Serialization.Mapping
{
    public class TestSystemServiceMap : IServiceMap
    {
        public OperationDefinition Definition { get; }

        public bool HasParameters => false;
        public bool HasXRoadFaultInResponse => true;

        public string ResponsePartName { get { throw new System.NotImplementedException(); } }

        public TestSystemServiceMap(XName operationName)
        {
            var methodInfo = typeof(Implementation).GetTypeInfo().GetMethod("Execute");

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

        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace)
        {
            throw new System.NotImplementedException();
        }

        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization = null)
        {
            var containsRequest = requestReader.MoveToElement(2, Definition.Name.LocalName, Definition.Name.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);
            else writer.WriteStartElement($"{Definition.Name.LocalName}Response", Definition.Name.NamespaceName);

            writer.WriteEndElement();
        }

        private class Implementation
        {
            public void Execute()
            { }
        }
    }
}