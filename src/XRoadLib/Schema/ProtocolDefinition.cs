using System;
using System.Xml;
using XRoadLib.Protocols.Styles;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Global configuration options for the schema.
    /// </summary>
    public class ProtocolDefinition
    {
        /// <summary>
        /// Serialization style used for X-Road message prtotocol.
        /// </summary>
        public Style Style { get; set; }

        /// <summary>
        /// Main namespace for the protocol.
        /// </summary>
        public string ProducerNamespace { get; set; }

        /// <summary>
        /// Callback function to be used to detect protocol from SOAP envelope element.
        /// </summary>
        public Func<XmlReader, bool> DetectEnvelope { get; set; }
    }
}