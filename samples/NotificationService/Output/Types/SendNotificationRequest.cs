using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MyNamespace
{
    public class SendNotificationRequest : IXmlSerializable
    {
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
        }
    }
}