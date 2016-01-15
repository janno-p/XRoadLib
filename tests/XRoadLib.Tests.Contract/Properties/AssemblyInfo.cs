using System.Reflection;
using XRoadLib;
using XRoadLib.Attributes;
using XRoadLib.Tests.Contract.Configuration;

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

[assembly: XRoadProducerName("test-producer")]

[assembly: XRoadSupportedProtocol(XRoadProtocol.Version20, Configuration = typeof(XRoadContractConfiguration))]
[assembly: XRoadSupportedProtocol(XRoadProtocol.Version31, Configuration = typeof(XRoadContractConfiguration))]
[assembly: XRoadSupportedProtocol(XRoadProtocol.Version40, Configuration = typeof(XRoadContractConfiguration))]

[assembly: XRoadTitle("", "Ilma keeleta palun")]
[assembly: XRoadTitle("en", "XRoadLib test producer")]
[assembly: XRoadTitle("et", "XRoadLib test andmekogu")]
[assembly: XRoadTitle("pt", "Portugalikeelne loba ...")]
