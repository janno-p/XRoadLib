using System.Web.Services.Description;
using XRoadLib.Protocols.Headers;

namespace XRoadLib.Protocols
{
    public class XRoad40Protocol : Protocol<XRoadHeader40>
    {
        public override string Name => "4.0";

        public XRoad40Protocol(string producerNamespace)
            : this(producerNamespace, new DocLiteralStyle())
        { }

        public XRoad40Protocol(string producerName, Style style)
            : base(producerName, style)
        { }

        protected override void DefineMandatoryHeaderElements()
        {
            AddMandatoryHeaderElement(x => x.Client);
            AddMandatoryHeaderElement(x => x.Service);
            AddMandatoryHeaderElement(x => x.UserId);
            AddMandatoryHeaderElement(x => x.Id);
            AddMandatoryHeaderElement(x => x.Issue);
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription, Context context)
        {
            var servicePort = serviceDescription.Services[0].Ports[0];

            var soapAddressBinding = (SoapAddressBinding)servicePort.Extensions[0];
            if (string.IsNullOrWhiteSpace(soapAddressBinding.Location))
                soapAddressBinding.Location = "http://INSERT_CORRECT_SERVICE_URL";
        }

        public override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XROAD_V4.Equals(ns) || NamespaceConstants.XROAD_V4_REPR.Equals(ns);
        }
    }
}