﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization;
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
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Func<XmlReader, Task<bool>> DetectEnvelopeAsync { get; set; }

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
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public string TechNotesElementName { get; set; } = "techNotes";

        /// <summary>
        /// Add fake documentation info for elements which do not have explicit documentation attributes defined.
        /// Fixes MISP issue when missing documentation causes MISP to display empty labels.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public bool GenerateFakeXRoadDocumentation { get; set; } = false;

        /// <summary>
        /// Port type name to use in service description document.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public string PortTypeName { get; set; } = "PortTypeName";

        /// <summary>
        /// Binding name to use in service description document.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public string BindingName { get; set; } = "BindingName";

        /// <summary>
        /// Port name used in service description document services section.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public string PortName { get; set; } = "PortName";

        /// <summary>
        /// Service name used in service description document.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public string ServiceName { get; set; } = "ServiceName";

        /// <summary>
        /// Soap address binding location value in service description document.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public string SoapAddressLocation { get; set; } = "http://INSERT_CORRECT_SERVICE_URL";

        /// <summary>
        /// Allows to define namespace prefixes which should be defined in outgoing SOAP messages.
        /// </summary>
        public IDictionary<XNamespace, string> GlobalNamespacePrefixes { get; } = new Dictionary<XNamespace, string>();

        /// <summary>
        /// Global empty tag handling mode for the protocol definition.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public EmptyTagHandlingMode EmptyTagHandlingMode { get; set; }
    }
}