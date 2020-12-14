using System;
using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class DateTestDto
    {
        [XmlElement(ElementName = "ttIsik.dSyn", DataType = "date")]
        public DateTime? Synniaeg { get; set; }
    }
}