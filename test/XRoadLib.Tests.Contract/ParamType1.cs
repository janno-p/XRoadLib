﻿namespace XRoadLib.Tests.Contract;

public class ParamType1 : XRoadSerializable
{
    [XmlElement(Order = 0)]
    public long Property1 { get; set; }

    [XRoadXmlArray(IsOptional = true, Order = 1)]
    public ParamType2[]? Property2 { get; set; }

    [XRoadXmlElement(IsOptional = true, Order = 2)]
    public string? Property3 { get; set; }

    [XRoadXmlElement(IsOptional = true, Order = 3)]
    public MimeContent? MimeContent { get; set; }
}