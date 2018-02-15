using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class XRoadMergeContentAttribute : Attribute
    { }
}