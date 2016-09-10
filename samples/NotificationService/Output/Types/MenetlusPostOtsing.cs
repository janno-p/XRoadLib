using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetlusPostOtsing : IXRoadXmlSerializable
    {
        public Option<DateTime?> EdasikaebamiseKP { get; set; }
        public Option<DateTime?> EdasikaebamiseKPVahemikuLoppKP { get; set; }
        public Option<bool?> Edasikaevatav { get; set; }
        public Option<Isik> Isik { get; set; }
        public Option<IList<Kattetoimetamine>> Kattetoimetamised { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<Menetlus> Menetlus { get; set; }
        public Option<DateTime?> NahtavuseKP { get; set; }
        public Option<bool?> NouabKattesaamist { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<bool?> OnKattesaadud { get; set; }
        public Option<bool?> OodatakseVastust { get; set; }
        public Option<bool?> VaatamisOiguseAndmine { get; set; }
        public Option<DateTime?> VastusnoudeKP { get; set; }
        public Option<DateTime?> VastusnoudeKPVahemikuLoppKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}