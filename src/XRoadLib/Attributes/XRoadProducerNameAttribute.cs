using System;

namespace XRoadLib.Attributes
{
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