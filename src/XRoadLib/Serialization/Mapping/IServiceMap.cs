using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace XRoadLib.Serialization.Mapping
{
    public interface IServiceMap
    {
        MethodInfo MethodInfo { get; }

        bool HasMultipartRequest { get; }

        bool HasMultipartResponse { get; }

        IDictionary<string, object> DeserializeRequest(XmlReader reader, SerializationContext context);

        object DeserializeResponse(XmlReader reader, SerializationContext context);

        void SerializeRequest(XmlWriter writer, IDictionary<string, object> values, SerializationContext context);

        void SerializeResponse(XmlWriter writer, object value, SerializationContext context, XmlReader requestReader, ICustomSerialization customSerialization = null);
    }
}