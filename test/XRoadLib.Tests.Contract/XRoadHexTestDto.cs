namespace XRoadLib.Tests.Contract;

public class XRoadHexTestDto : XRoadSerializable
{
    [XRoadXmlElement(DataType = "hexBinary", UseXop = false)]
    public Stream? Sisu { get; set; }
}