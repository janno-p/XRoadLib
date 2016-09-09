using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaToovoimetuteKaristusedRequest : IXRoadXmlSerializable
    {
        public Isik Jareleparija { get; set; }
        public Isik Kasutaja { get; set; }

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
        public KARRParinguAndmed ParinguAndmed { get; set; }
        public Option<long> AsjaLiikKL { get; set; }
        public DateTime KohustiseRakendumiseAlgusKP { get; set; }
        public DateTime KohustiseRakendumiseAlgusKPVahemikuLoppKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}