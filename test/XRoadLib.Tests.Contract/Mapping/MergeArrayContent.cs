using JetBrains.Annotations;

namespace XRoadLib.Tests.Contract.Mapping;

[XmlType(AnonymousType = true)]
public class WrapperType : XRoadSerializable
{
    [XmlArrayItem("Integer")]
    [XRoadMergeContent]
    public int[]? Integers { get; set; }

    [XmlArrayItem("String")]
    [XRoadMergeContent]
    public string[]? Strings { get; set; }
}

public class MergeArrayContentRequest : XRoadSerializable
{
    [XmlElement(Order = 1, DataType = "date")]
    public DateTime StartDate { get; set; }

    [XmlElement(Order = 2, DataType = "date")]
    public DateTime EndDate { get; set; }

    [XmlArray(Order = 3)]
    [XRoadMergeContent]
    public WrapperType[]? Content { get; set; }
}

[UsedImplicitly]
public interface IMergeArrayContentService
{
    [XRoadService("MergeArrayContent")]
    void Service(MergeArrayContentRequest request);
}