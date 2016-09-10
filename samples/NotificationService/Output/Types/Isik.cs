using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public abstract class Isik : IXRoadXmlSerializable
    {
        public Option<bool?> AinultPohiandmed { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<string> EelnevadKoosnimedCSV { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<DateTime?> KaristusteYlevaatamiseKP { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<IList<Kohustis>> Kohustised { get; set; }
        public Option<IList<Kontakt>> Kontaktid { get; set; }
        public Option<IList<Konto>> Kontod { get; set; }
        public Option<string> Kood { get; set; }
        public Option<string> Koosnimi { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<string> Markused { get; set; }
        public Option<long?> MenetlusSubjektiLiikKL { get; set; }
        public Option<string> MuudeRiikideKoodid { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<string> Nimi { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<bool> OnValideeritudAktuaalneVersioon { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<IList<long>> TegevusalaKL { get; set; }
        public Option<DateTime?> VerAlgusKP { get; set; }
        public Option<DateTime?> VerLoppKP { get; set; }
        public Option<long> VersID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}