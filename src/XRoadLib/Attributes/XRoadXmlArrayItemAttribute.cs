using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using XRoadLib.Serialization;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Represents an attribute that specifies the derived types that the serializer can place in a serialized array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class XRoadXmlArrayItemAttribute : XmlArrayItemAttribute
    {
        /// <summary>
        /// Minimum number of times this element must occur in serialized XML.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public uint MinOccurs { get; set; } = 0u;
        
        /// <summary>
        /// Maximum number of times this element can occur in serialized XML.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public uint? MaxOccurs { get; set; }

        /// <summary>
        /// Use MTOM/XOP standard for binary content serialization (default value is `true`).
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public bool UseXop { get; set; } = true;

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public EmptyTagHandlingMode? EmptyTagHandlingMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public XRoadXmlArrayItemAttribute()
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class and specifies the name of the XML element
        /// generated in the XML document.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public XRoadXmlArrayItemAttribute(string elementName)
            : base(elementName)
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class and specifies the name of the XML element
        /// generated in the XML document and the Type that can be inserted into the generated XML document.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="type">The Type of the object to serialize.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public XRoadXmlArrayItemAttribute(string elementName, Type type)
            : base(elementName, type)
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class and specifies the Type that can be inserted
        /// into the serialized array.
        /// </summary>
        /// <param name="type">The Type of the object to serialize.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public XRoadXmlArrayItemAttribute(Type type)
            : base(type)
        { }
    }
}