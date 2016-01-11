using System;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Identifies producer which types and service contracts are defined inside attribute target assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class XRoadProducerNameAttribute : Attribute
    {
        public string Value { get; }

        public XRoadProducerNameAttribute(string value)
        {
            Value = value;
        }
    }
}