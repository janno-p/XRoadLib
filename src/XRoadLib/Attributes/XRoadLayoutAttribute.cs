using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    class XRoadLayoutAttribute : Attribute
    {
        internal XRoadProtocol? appliesTo;

        public XRoadProtocol AppliesTo { get { return appliesTo.GetValueOrDefault(); } set { appliesTo = value; } }

        public XRoadLayoutKind Layout { get; set; }

        public Type Comparer { get; set; }
    }
}
