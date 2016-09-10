using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaIsikuKaristusFaktidRequest : IXRoadXmlSerializable
    {
        public Isik Jareleparija { get; set; }
        public Isik Kasutaja { get; set; }
        public Isik KontrollitavIsik { get; set; }
        public Option<Maksekorraldus> Maksekorraldus { get; set; }
        public Option<KARRParinguAndmed> ParinguAndmed { get; set; }
        public Option<IList<string>> KvalifikatsiooniPeatykiNimetusKL { get; set; }
        public Option<IList<string>> KvalifikatsiooniObjektID { get; set; }
        public bool OtsiArhiivist { get; set; }
        public Option<bool?> OnPiiratudParing { get; set; }
        public Option<long?> ParinguID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}