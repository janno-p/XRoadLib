using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class XRoadMergeContentAttribute : Attribute
    { }
}