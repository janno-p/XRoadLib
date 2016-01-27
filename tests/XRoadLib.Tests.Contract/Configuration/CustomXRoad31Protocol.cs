using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Protocols;
using XRoadLib.Schema;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomXRoad31Protocol : XRoad31Protocol
    {
        public static IProtocol Instance { get; } = new CustomXRoad31Protocol();

        private CustomXRoad31Protocol()
            : base("test-producer", "http://test-producer.x-road.ee/producer/")
        {
            Titles.Add("", "Ilma keeleta palun");
            Titles.Add("en", "XRoadLib test producer");
            Titles.Add("et", "XRoadLib test andmekogu");
            Titles.Add("pt", "Portugalikeelne loba ...");
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

        public override void ExportOperation(OperationDefinition operation)
        {
            base.ExportOperation(operation);

            // Customize root type name for operations (when applicable):
            operation.RequestTypeName = XName.Get($"{operation.Name.LocalName}Request", operation.RequestTypeName.NamespaceName);
            operation.ResponseTypeName = XName.Get($"{operation.Name.LocalName}Response", operation.RequestTypeName.NamespaceName);

            // Customize operation message names:
            operation.RequestMessageName = operation.Name.LocalName;
            operation.ResponseMessageName = $"{operation.Name.LocalName}Response";
        }

        public override void ExportType(TypeDefinition type)
        {
            base.ExportType(type);

            // Customize type content model:
            if (type.RuntimeInfo == typeof(ParamType1))
                type.HasStrictContentOrder = false;
        }
    }
}
 