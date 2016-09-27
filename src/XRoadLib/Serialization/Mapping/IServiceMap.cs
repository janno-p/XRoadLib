using System.Xml;
using XRoadLib.Schema;

namespace XRoadLib.Serialization.Mapping
{
    /// <summary>
    /// Provides serialization/deserialization interface for X-Road operations.
    /// </summary>
    public interface IServiceMap
    {
        /// <summary>
        /// Configuration settings of the operation that the ServiceMap implements.
        /// </summary>
        OperationDefinition OperationDefinition { get; }

        /// <summary>
        /// Response element specification of the X-Road operation.
        /// </summary>
        RequestValueDefinition RequestValueDefinition { get; }

        /// <summary>
        /// Response element specification of the X-Road operation.
        /// </summary>
        ResponseValueDefinition ResponseValueDefinition { get; }

        /// <summary>
        /// Deserializes X-Road message protocol requests according to operation definitions.
        /// </summary>
        object DeserializeRequest(XmlReader reader, XRoadMessage message);

        /// <summary>
        /// Deserializes X-Road message protocol responses according to operation definitions.
        /// </summary>
        object DeserializeResponse(XmlReader reader, XRoadMessage message);

        /// <summary>
        /// Serializes X-Road message protocol requests according to operation definitions.
        /// </summary>
        void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace = null);

        /// <summary>
        /// Serializes X-Road message protocol responses according to operation definitions.
        /// </summary>
        void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization = null);
    }
}