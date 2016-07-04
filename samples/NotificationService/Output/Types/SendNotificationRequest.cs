using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    class SendNotificationRequest : IXRoadXmlSerializable
    {
        public class QualificationsType : IXRoadXmlSerializable
        {
            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public class ObligationsType : IXRoadXmlSerializable
        {
            public class itemType : IXRoadXmlSerializable
            {
                void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
                {
                }

                void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
                {
                }
            }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}