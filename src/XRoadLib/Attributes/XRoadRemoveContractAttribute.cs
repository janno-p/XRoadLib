using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Struct)]
    public class XRoadRemoveContractAttribute : Attribute
    {
        public uint Version { get; set; }
        public Type Converter { get; set; }
    }
}