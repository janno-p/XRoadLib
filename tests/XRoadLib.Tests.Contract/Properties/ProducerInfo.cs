using XRoadLib.Attributes;

[assembly: XRoadProducerConfiguration(
    ProducerName = "test-producer",
    MinOperationVersion = 1u,
    MaxOperationVersion = 2u,
    StandardHeaderName = "stdhdr",
    PortTypeName = "TestProducerPortType",
    BindingName = "TestBinding",
    ServicePortName = "TestPort",
    ServiceName = "TestService",
    RequestTypeNameFormat = "{0}Request",
    ResponseTypeNameFormat = "{0}Response",
    RequestMessageNameFormat = "{0}",
    ResponseMessageNameFormat = "{0}Response",
    StrictOperationSignature = true)]

[assembly: XRoadTitle("", "Ilma keeleta palun")]
[assembly: XRoadTitle("en", "XRoadLib test producer")]
[assembly: XRoadTitle("et", "XRoadLib test andmekogu")]
[assembly: XRoadTitle("pt", "Portugalikeelne loba ...")]
