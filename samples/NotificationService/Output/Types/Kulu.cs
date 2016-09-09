using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kulu : IXRoadXmlSerializable
    {
        public Option<long> AlaLiikKL { get; set; }
        public Option<DateTime> AlgusKP { get; set; }

        public class AluseksOlevadNoudedType : IXRoadXmlSerializable
        {
            public Option<Noue> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AluseksOlevadNoudedType> AluseksOlevadNouded { get; set; }

        public class AluselTekkinudNoudedType : IXRoadXmlSerializable
        {
            public Option<Noue> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AluselTekkinudNoudedType> AluselTekkinudNouded { get; set; }

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

        public class KandmiseOsadType : IXRoadXmlSerializable
        {
            public Option<KuluOsa> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KandmiseOsadType> KandmiseOsad { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<Isik> KuluSaajaIsik { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<Menetlus> Menetlus { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime> MuutmiseKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<long> PohjustamiseJagamiseViisKL { get; set; }

        public class PohjustamiseOsadType : IXRoadXmlSerializable
        {
            public Option<KuluOsa> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<PohjustamiseOsadType> PohjustamiseOsad { get; set; }
        public Option<string> Selgitus { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime> SisestamiseKP { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }
        public Option<decimal> Summa { get; set; }
        public Option<DateTime> TekkimiseKP { get; set; }
        public Option<Toiming> Toiming { get; set; }
        public Option<long> TyypKL { get; set; }
        public Option<long> ValuutaKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}