﻿using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Protocols;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomXRoad20Protocol : XRoad20Protocol
    {
        private const string producerName = "test-producer";
        private const string producerNamespace = "http://producers.test-producer.xtee.riik.ee/producer/test-producer";

        public static IProtocol Instance { get; } = new CustomXRoad20Protocol();

        private CustomXRoad20Protocol()
            : base(producerName, producerNamespace, null, new CustomSchemaExporter(producerNamespace))
        {
            Titles.Add("", "Ilma keeleta palun");
            Titles.Add("en", "XRoadLib test producer");
            Titles.Add("et", "XRoadLib test andmekogu");
            Titles.Add("pt", "Portugalikeelne loba ...");

            SetContractAssembly(GetType().Assembly, 1u, 2u, 3u);
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
 