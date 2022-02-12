﻿using XRoadLib.Serialization;

namespace XRoadLib.Attributes;

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

    [UsedImplicitly]
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
    [UsedImplicitly]
    public XRoadXmlArrayAttribute(string elementName)
        : base(elementName)
    { }
}