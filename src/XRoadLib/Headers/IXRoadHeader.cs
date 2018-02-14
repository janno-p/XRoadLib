using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Styles;

namespace XRoadLib.Headers
{
    /// <summary>
    /// Describes X-Road message SOAP header elements.
    /// </summary>
    public interface IXRoadHeader
    {
        /// <summary>
        /// Identifies X-Road client.
        /// </summary>
        XRoadClientIdentifier Client { get; }

        /// <summary>
        /// Identifies X-Road operation.
        /// </summary>
        XRoadServiceIdentifier Service { get; }

        /// <summary>
        /// Identifies user who sent X-Road message.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Unique id for the X-Road message.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Operation specific identifier for the X-Road message.
        /// </summary>
        string Issue { get; }

        /// <summary>
        /// Protocol version of the X-Road message.
        /// </summary>
        string ProtocolVersion { get; }

        /// <summary>
        /// Try to read current position in XML reader as X-Road header element.
        /// </summary>
        void ReadHeaderValue(XmlReader reader);

        /// <summary>
        /// Check if all required SOAP headers are present and in correct format.
        /// </summary>
        void Validate();

        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        void WriteTo(XmlWriter writer, Style style, HeaderDefinition headerDefinition);
    }
}