using System.Reflection;
using XRoadLib.Attributes;

[assembly: AssemblyTitle("XRoadLib.Tests.Contract")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("XRoadLib.Tests.Contract")]
[assembly: AssemblyCopyright("Copyright ©  2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: XRoadProducerConfiguration(ProducerName = "test-producer",
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
