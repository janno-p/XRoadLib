using System;
using System.Xml.Serialization;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Indicates that a public field or property represents an XML element when the serializer serializes or
    /// deserializes the object that contains it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true)]
    public class XRoadXmlElementAttribute : XmlElementAttribute
    {
        /// <summary>
        /// Indicates if this element must occur in serialized XML or not.
        /// Is internally controlled by MinOccurs property so that
        /// `true` value specifies MinOccurs value `0` and `false` value
        /// specifies MinOccurs value `1`.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Use MTOM/XOP standard for binary content serialization (default value is `true`).
        /// </summary>
        public bool UseXop { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the XRoadXmlElementAttribute class.
        /// </summary>
        public XRoadXmlElementAttribute()
        { }
        
        /// <summary>
        /// Initializes a new instance of the XRoadXmlElementAttribute class and specifies the name of the XML element.
        /// <param name="elementName">The XML element name of the serialized member.</param>
        /// </summary>
        public XRoadXmlElementAttribute(string elementName)
            : base(elementName)
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlElementAttribute and specifies the name of the XML element and a
        /// derived type for the member to which the XRoadXmlElementAttribute is applied.
        /// </summary>
        /// <param name="elementName">The XML element name of the serialized member.</param>
        /// <param name="type">The Type of an object derived from the member's type.</param>
        public XRoadXmlElementAttribute(string elementName, Type type)
            : base(elementName, type)
        { }
        
        /// <summary>
        /// Initializes a new instance of the XmlElementAttribute class and specifies a type for the member to which
        /// the XmlElementAttribute is applied.
        /// </summary>
        /// <param name="type">The Type of an object derived from the member's type.</param>
        public XRoadXmlElementAttribute(Type type)
            : base(type)
        { }
    }
}