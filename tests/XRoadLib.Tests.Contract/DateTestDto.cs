using System;
using System.Xml.Serialization;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class DateTestDto : XRoadSerializable
    {
        [XmlElement(ElementName = "ttIsik.dSyn", DataType = "date")]
        public DateTime? Synniaeg { get; set; }
    }
}