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
        /// Provides information whether this ServiceMap has any input parameters defined
        /// or has an empty request element.
        /// </summary>
        bool HasParameters { get; }

        /// <summary>
        /// Specifies if X-Road fault is returned wrapped inside operation response element
        /// or separately as its own element.
        /// </summary>
        bool HasXRoadFaultInResponse { get; }

        /// <summary>
        /// Response part name of the operation.
        /// </summary>
        string ResponsePartName { get; }

        /// <summary>
        /// Configuration settings of the operation that the ServiceMap implements.
        /// </summary>
        OperationDefinition Definition { get; }

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