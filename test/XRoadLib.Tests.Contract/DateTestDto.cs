namespace XRoadLib.Tests.Contract;

public class DateTestDto : XRoadSerializable
{
    [XmlElement(ElementName = "ttIsik.dSyn", DataType = "date")]
    public DateTime? Synniaeg { get; set; }
}