using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.ReturnValue)]
    public class XRoadOptionalAttribute : Attribute
    { }
}