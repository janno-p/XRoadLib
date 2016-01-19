using System.Xml.Serialization;

namespace XRoadLib.Soap
{
    [XmlType("Fault", Namespace = NamespaceConstants.SOAP_ENV)]
    public interface ISoapFault
    {
        [XmlElement("faultcode")]
        string FaultCode { get; }

        [XmlElement("faultstring")]
        string FaultString { get; }

        [XmlElement("faultactor")]
        string FaultActor { get; }

        [XmlElement("detail")]
        string Details { get; }
    }
}