using System.Xml;

namespace XRoadLib.Protocols.Headers
{
    public interface IXRoadHeader
    {
        XRoadClientIdentifier Client { get; }
        XRoadServiceIdentifier Service { get; }

        string UserId { get; }
        string Id { get; }
        string Issue { get; }
        string ProtocolVersion { get; }

        void SetHeaderValue(XmlReader reader);

        void Validate();
    }
}