using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using XRoadLib.Serialization;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Specifies that the serializer must serialize a particular class member as an array of XML elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class XRoadXmlArrayAttribute : XmlArrayAttribute
    {
        /// <summary>
        /// Indicates if this element must occur in serialized XML or not.
        /// </summary>
        public bool IsOptional { get; set; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public EmptyTagHandlingMode? EmptyTagHandlingMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the XmlArrayAttribute class.
        /// </summary>
        public XRoadXmlArrayAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the XmlArrayAttribute class and specifies the XML element name generated
        /// in the XML document instance.
        /// </summary>
        /// <param name="elementName">The XML element name of the serialized member.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public XRoadXmlArrayAttribute(string elementName)
            : base(elementName)
        { }
    }
}