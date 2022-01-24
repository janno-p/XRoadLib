namespace XRoadLib.Tests.Contract.Wsdl;

[XmlType(AnonymousType = true)]
public class AnonymousType : XRoadSerializable
{
    public string? Property1 { get; set; }
    public string? Property2 { get; set; }
    public string? Property3 { get; set; }
}

public class ContainerType : XRoadSerializable
{
    public AnonymousType? AnonymousProperty { get; set; }
    public string? KnownProperty { get; set; }
}