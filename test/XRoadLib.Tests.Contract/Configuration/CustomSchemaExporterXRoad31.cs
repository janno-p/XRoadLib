using System.Linq;
using System.Reflection;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Wsdl;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomSchemaExporterXRoad31 : SchemaExporterXRoad31
    {
        public CustomSchemaExporterXRoad31()
            : base("test-producer", typeof(Class1).GetTypeInfo().Assembly)
        { }

        public override void ExportOperationDefinition(OperationDefinition operationDefinition)
        {
            // Customize operation message names:
            operationDefinition.InputMessageName = operationDefinition.Name.LocalName;
            operationDefinition.OutputMessageName = $"{operationDefinition.Name.LocalName}Response";
        }

        public override void ExportTypeDefinition(TypeDefinition typeDefinition)
        {
            // Customize type content model:
            if (typeDefinition.Type == typeof(ParamType1))
                typeDefinition.HasStrictContentOrder = false;
        }

        public override void ExportPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            propertyDefinition.Content.UseXop = false;
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            // Customize port type name:
            serviceDescription.PortTypes[0].Name = "TestProducerPortType";
            serviceDescription.Bindings[0].Type = new XmlQualifiedName("TestProducerPortType", producerNamespace);

            // Customize binding name:
            serviceDescription.Bindings[0].Name = "TestBinding";
            serviceDescription.Services[0].Ports[0].Binding = new XmlQualifiedName("TestBinding", producerNamespace);

            // Customize service port name:
            var servicePort = serviceDescription.Services[0].Ports[0];
            servicePort.Name = "TestPort";

            // Customize service name:
            serviceDescription.Services[0].Name = "TestService";
        }

        protected override void AddXRoadTitles(Port servicePort)
        {
            AddXRoadTitle(servicePort, "", "Ilma keeleta palun");
            AddXRoadTitle(servicePort, "en", "XRoadLib test producer");
            AddXRoadTitle(servicePort, "et", "XRoadLib test andmekogu");
            AddXRoadTitle(servicePort, "pt", "Portugalikeelne loba ...");
        }

        public override void ExportProtocolDefinition(ProtocolDefinition protocolDefinition)
        {
            base.ExportProtocolDefinition(protocolDefinition);

            foreach (var version in Enumerable.Range(1, 3))
                protocolDefinition.SupportedVersions.Add((uint)version);
        }
    }
}
