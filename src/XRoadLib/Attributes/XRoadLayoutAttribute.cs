using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class XRoadLayoutAttribute : Attribute, IXRoadProtocolAppliable
    {
        private XRoadProtocol? appliesTo;

        public bool HasAppliesToValue => appliesTo.HasValue;

        public XRoadProtocol AppliesTo { get { return appliesTo.GetValueOrDefault(); } set { appliesTo = value; } }

        public XRoadPropertyOrder PropertyOrder { get; set; }

        public Type Comparer { get; set; }
    }
}
