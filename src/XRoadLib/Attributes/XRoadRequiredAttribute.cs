using System;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// By default every element is optional (minOccurs = 0).
    /// By using `XRoadRequired` attribute, element is made mandatory in XML message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class XRoadRequiredAttribute : Attribute
    { }
}