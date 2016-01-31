using System.Collections.Generic;
using System.Xml;
using XRoadLib.Schema;

namespace XRoadLib.Serialization.Mapping
{
    public interface IServiceMap
    {
        OperationDefinition OperationDefinition { get; }

        IDictionary<string, object> DeserializeRequest(XmlReader reader, XRoadMessage message);

        object DeserializeResponse(XmlReader reader, XRoadMessage message);

        void SerializeRequest(XmlWriter writer, IDictionary<string, object> values, XRoadMessage message);

        void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization = null);
    }
}