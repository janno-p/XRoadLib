using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SendNotificationRequest : IXRoadXmlSerializable
    {
        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}