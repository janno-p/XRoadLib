using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Struct)]
    public class XRoadAddContractAttribute : Attribute
    {
        public uint Version { get; set; }
    }
}
