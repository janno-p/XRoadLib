using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Protocols;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomXRoad31Protocol : XRoad31Protocol
    {
        public static XRoadProtocol Instance { get; } = new CustomXRoad31Protocol();

        private CustomXRoad31Protocol()
            : base("test-producer", "http://test-producer.x-road.ee/producer/", null, new SchemaExporter())
        {
            Titles.Add("", "Ilma keeleta palun");
            Titles.Add("en", "XRoadLib test producer");
            Titles.Add("et", "XRoadLib test andmekogu");
            Titles.Add("pt", "Portugalikeelne loba ...");

            SetContractAssembly(GetType().Assembly, null, 1u, 2u, 3u);
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            // Customize port type name:
            serviceDescription.PortTypes[0].Name = "TestProducerPortType";
            serviceDescription.Bindings[0].Type = new XmlQualifiedName("TestProducerPortType", ProducerNamespace);

            // Customize binding name:
            serviceDescription.Bindings[0].Name = "TestBinding";
            serviceDescription.Services[0].Ports[0].Binding = new XmlQualifiedName("TestBinding", ProducerNamespace);

            // Customize service port name:
            serviceDescription.Services[0].Ports[0].Name = "TestPort";

            // Customize service name:
            serviceDescription.Services[0].Name = "TestService";
        }
    }
}
 