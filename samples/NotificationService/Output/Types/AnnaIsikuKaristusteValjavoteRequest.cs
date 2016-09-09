using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class AnnaIsikuKaristusteValjavoteRequest : IXRoadXmlSerializable
    {
        public class KontrollitavIsikType : IXRoadXmlSerializable
        {
            public Option<Isik> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public KontrollitavIsikType KontrollitavIsik { get; set; }
        public Isik Jareleparija { get; set; }
        public Isik Kasutaja { get; set; }
        public Maksekorraldus Maksekorraldus { get; set; }
        public KarrMeta Piirangud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}