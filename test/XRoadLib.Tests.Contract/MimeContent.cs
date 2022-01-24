namespace XRoadLib.Tests.Contract;

public class MimeContent : XRoadSerializable
{
    [XRoadXmlElement(UseXop = false)]
    public Stream? Value { get; set; }
}