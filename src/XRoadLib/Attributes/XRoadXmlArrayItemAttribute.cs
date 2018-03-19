using System;
using System.Xml.Serialization;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Represents an attribute that specifies the derived types that the serializer can place in a serialized array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true)]
    public class XRoadXmlArrayItemAttribute : XmlArrayItemAttribute
    {
        /// <summary>
        /// Minimum number of times this element must occur in serialized XML.
        /// </summary>
        public uint MinOccurs { get; set; } = 0u;
        
        /// <summary>
        /// Maximum number of times this element can occur in serialized XML.
        /// </summary>
        public uint? MaxOccurs { get; set; }

        /// <summary>
        /// Use MTOM/XOP standard for binary content serialization (default value is `true`).
        /// </summary>
        public bool UseXop { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class.
        /// </summary>
        public XRoadXmlArrayItemAttribute()
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class and specifies the name of the XML element
        /// generated in the XML document.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        public XRoadXmlArrayItemAttribute(string elementName)
            : base(elementName)
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class and specifies the name of the XML element
        /// generated in the XML document and the Type that can be inserted into the generated XML document.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="type">The Type of the object to serialize.</param>
        public XRoadXmlArrayItemAttribute(string elementName, Type type)
            : base(elementName, type)
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlArrayItemAttribute class and specifies the Type that can be inserted
        /// into the serialized array.
        /// </summary>
        /// <param name="type">The Type of the object to serialize.</param>
        public XRoadXmlArrayItemAttribute(Type type)
            : base(type)
        { }
    }
}