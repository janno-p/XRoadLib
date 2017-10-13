using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    public enum TestEnum
    {
        [XmlEnum("Value 1")]
        [XRoadTitle("Value 1")]
        Value1,

        [XmlEnum("Value 3")]
        [XRoadTitle("Value 1")]
        Value2,

        [XmlEnum("Value 3")]
        [XRoadTitle("Value 1")]
        Value3
    }
}