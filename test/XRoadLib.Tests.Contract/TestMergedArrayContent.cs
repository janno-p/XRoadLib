namespace XRoadLib.Tests.Contract;

public class TestMergedArrayContent : XRoadSerializable
{
    [XmlElement(Order = 1)]
    public string? Value { get; set; }

    [XmlArray(Order = 2)]
    [XmlArrayItem("Code")]
    [XRoadMergeContent]
    public string[]? Codes { get; set; }

    [XmlElement(Order = 3)]
    public string? Value2 { get; set; }
}