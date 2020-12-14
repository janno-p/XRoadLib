using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class XRoadSerializableAttribute : Attribute { }
}