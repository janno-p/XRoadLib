using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaIsikuteKehtivadKaristusedRequest : IXRoadXmlSerializable
    {
        public Isik Jareleparija { get; set; }
        public Isik Kasutaja { get; set; }

        public class KontrollitavIsikType : IXRoadXmlSerializable
        {
            public IList<Isik> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public KontrollitavIsikType KontrollitavIsik { get; set; }
        public Option<Maksekorraldus> Maksekorraldus { get; set; }
        public KARRParinguAndmed ParinguAndmed { get; set; }

        public class KvalifikatsiooniPeatykiNimetusKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KvalifikatsiooniPeatykiNimetusKLType> KvalifikatsiooniPeatykiNimetusKL { get; set; }

        public class KvalifikatsiooniObjektIDType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KvalifikatsiooniObjektIDType> KvalifikatsiooniObjektID { get; set; }
        public Option<boolean> OnPiiratudParing { get; set; }
        public Option<long?> AsjaLiikKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}