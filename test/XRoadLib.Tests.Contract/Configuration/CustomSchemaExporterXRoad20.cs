using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomSchemaExporterXRoad20 : SchemaExporterXRoad20
    {
        private readonly StringSerializationMode stringSerializationMode;

        public CustomSchemaExporterXRoad20(StringSerializationMode stringSerializationMode = StringSerializationMode.HtmlEncoded)
            : base("test-producer")
        {
            this.stringSerializationMode = stringSerializationMode;
        }

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
            propertyDefinition.UseXop = false;
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
            var servicePort = serviceDescription.Services[0].Ports[0];
            servicePort.Name = "TestPort";

            // Customize service name:
            serviceDescription.Services[0].Name = "TestService";

            AddXRoadTitle(servicePort, "", "Ilma keeleta palun");
            AddXRoadTitle(servicePort, "en", "XRoadLib test producer");
            AddXRoadTitle(servicePort, "et", "XRoadLib test andmekogu");
            AddXRoadTitle(servicePort, "pt", "Portugalikeelne loba ...");
        }

        public override void ExportProtocolDefinition(ProtocolDefinition protocolDefinition)
        {
            base.ExportProtocolDefinition(protocolDefinition);

            if (stringSerializationMode == StringSerializationMode.WrappedInCData)
                protocolDefinition.Style = new CDataRpcEncodedStyle();
        }

        private class CDataRpcEncodedStyle : RpcEncodedStyle
        {
            public override StringSerializationMode StringSerializationMode => StringSerializationMode.WrappedInCData;
        }
    }
}
