using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Isik : IXRoadXmlSerializable
    {
        public Option<boolean> AinultPohiandmed { get; set; }
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<string> EelnevadKoosnimedCSV { get; set; }

        public class HoiatusedType : IXRoadXmlSerializable
        {
            public Option<ETHoiatus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<HoiatusedType> Hoiatused { get; set; }
        public Option<DateTime> KaristusteYlevaatamiseKP { get; set; }
        public Option<string> KlientsysteemiID { get; set; }

        public class KohustisedType : IXRoadXmlSerializable
        {
            public Option<Kohustis> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KohustisedType> Kohustised { get; set; }

        public class KontaktidType : IXRoadXmlSerializable
        {
            public Option<Kontakt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KontaktidType> Kontaktid { get; set; }

        public class KontodType : IXRoadXmlSerializable
        {
            public Option<Konto> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KontodType> Kontod { get; set; }
        public Option<string> Kood { get; set; }
        public Option<string> Koosnimi { get; set; }
        public Option<DateTime> KoostamiseKP { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<string> Markused { get; set; }
        public Option<long> MenetlusSubjektiLiikKL { get; set; }
        public Option<string> MuudeRiikideKoodid { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime> MuutmiseKP { get; set; }
        public Option<string> Nimi { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<boolean> OnValideeritudAktuaalneVersioon { get; set; }
        public Option<long> SeisundKL { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime> SisestamiseKP { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }

        public class TegevusalaKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TegevusalaKLType> TegevusalaKL { get; set; }
        public Option<DateTime> VerAlgusKP { get; set; }
        public Option<DateTime> VerLoppKP { get; set; }
        public Option<long> VersID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}