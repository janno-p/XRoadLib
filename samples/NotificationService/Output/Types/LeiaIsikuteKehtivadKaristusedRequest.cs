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
        public IList<Isik> KontrollitavIsik { get; set; }
        public Option<Maksekorraldus> Maksekorraldus { get; set; }
        public KARRParinguAndmed ParinguAndmed { get; set; }
        public Option<IList<long>> KvalifikatsiooniPeatykiNimetusKL { get; set; }
        public Option<IList<long>> KvalifikatsiooniObjektID { get; set; }
        public Option<bool?> OnPiiratudParing { get; set; }
        public Option<long?> AsjaLiikKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}