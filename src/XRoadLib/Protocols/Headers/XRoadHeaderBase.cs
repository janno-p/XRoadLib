using System.Xml;

namespace XRoadLib.Protocols.Headers
{
    public abstract class XRoadHeaderBase : IXRoadHeader
    {
        public XRoadClientIdentifier Client { get; internal set; }
        public XRoadServiceIdentifier Service { get; internal set; }

        public string UserId { get; set; }
        public string Id { get; set; }
        public string Issue { get; set; }
        public string ProtocolVersion { get; set; }

        public abstract void SetHeaderValue(XmlReader reader);

        public virtual void Validate()
        { }
    }
}