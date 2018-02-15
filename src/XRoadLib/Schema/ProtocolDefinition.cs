using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using XRoadLib.Styles;

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

        /// <summary>
        /// Assembly that defines types for serialization.
        /// </summary>
        public Assembly ContractAssembly { get; set; }

        /// <summary>
        /// Defines list of supported DTO versions (for DTO based versioning).
        /// </summary>
        public ISet<uint> SupportedVersions { get; } = new HashSet<uint>();

        /// <summary>
        /// Define list of content filters of X-Road message elements.
        /// </summary>
        public ISet<string> EnabledFilters { get; } = new HashSet<string>();

        /// <summary>
        /// Defines technotes element name
        /// </summary>
        public string TechNotesElementName { get; set; } = "techNotes";

        /// <summary>
        /// Add fake documentation info for elements which do not have explicit documentation attributes defined.
        /// Fixes MISP issue when missing documentation causes MISP to display empty labels.
        /// </summary>
        public bool GenerateFakeXRoadDocumentation { get; set; } = false;
    }
}