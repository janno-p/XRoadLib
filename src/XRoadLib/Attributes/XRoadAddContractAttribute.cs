using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct)]
    public class XRoadAddContractAttribute : Attribute
    {
        public uint Version { get; set; }
    }
}