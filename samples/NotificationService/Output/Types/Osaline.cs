using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Osaline : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }

        public class EsindajadType : IXRoadXmlSerializable
        {
            public Option<Osaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<EsindajadType> Esindajad { get; set; }

        public class EsindatavadType : IXRoadXmlSerializable
        {
            public Option<Osaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<EsindatavadType> Esindatavad { get; set; }
        public Option<string> Haigestumine { get; set; }

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
        public Option<Isik> Isik { get; set; }
        public Option<string> IsikCSV { get; set; }
        public Option<string> IsikuEritunnusteKirjeldus { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<DateTime> KoostamiseKP { get; set; }

        public class KvalifikatsioonType : IXRoadXmlSerializable
        {
            public Option<KvalifikatsiooniParagrahv> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KvalifikatsioonType> Kvalifikatsioon { get; set; }
        public Option<string> KvalifikatsioonCSV { get; set; }
        public Option<string> Leppenimi { get; set; }
        public Option<long> LiikAsjasKL { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<string> Markused { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime> MuutmiseKP { get; set; }

        public class NoudedType : IXRoadXmlSerializable
        {
            public Option<Noue> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<NoudedType> Nouded { get; set; }

        public class ObjektidType : IXRoadXmlSerializable
        {
            public Option<Objekt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ObjektidType> Objektid { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> ObjektideLoetelu { get; set; }
        public Option<string> Ohtlikkus { get; set; }
        public Option<string> OsalistIseloomustavadAndmed { get; set; }
        public Option<decimal> PaevaSissetulek { get; set; }
        public Option<DateTime> PaevaSissetulekKP { get; set; }
        public Option<long> PaevaSissetulekValuutaKL { get; set; }
        public Option<boolean> PoleMenetlussePuutuv { get; set; }

        public class RikutudOigusnormType : IXRoadXmlSerializable
        {
            public Option<KvalifikatsiooniParagrahv> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<RikutudOigusnormType> RikutudOigusnorm { get; set; }
        public Option<string> RikutudOigusnormCSV { get; set; }
        public Option<long> SalastatuseTaseKL { get; set; }

        public class SanktsioonidType : IXRoadXmlSerializable
        {
            public Option<Sanktsioon> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SanktsioonidType> Sanktsioonid { get; set; }

        public class SeotudAsjadType : IXRoadXmlSerializable
        {
            public Option<Menetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeotudAsjadType> SeotudAsjad { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime> SisestamiseKP { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }
        public Option<string> SundraviVajadus { get; set; }
        public Option<string> SyydIseloomustab { get; set; }
        public Option<string> Syydistus { get; set; }

        public class SyydistusPunktidType : IXRoadXmlSerializable
        {
            public Option<SyydistusPunkt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SyydistusPunktidType> SyydistusPunktid { get; set; }

        public class SyydToendavadToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SyydToendavadToimingudType> SyydToendavadToimingud { get; set; }
        public Option<string> SyydToendavateToiminguteLoetelu { get; set; }

        public class SyyteosyndmusedType : IXRoadXmlSerializable
        {
            public Option<Syyteosyndmus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SyyteosyndmusedType> Syyteosyndmused { get; set; }
        public Option<string> ToimikuLkNR { get; set; }

        public class ToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingudType> Toimingud { get; set; }
        public Option<string> VaimneSeisund { get; set; }

        public class VaradType : IXRoadXmlSerializable
        {
            public Option<Objekt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<VaradType> Varad { get; set; }
        public Option<string> VaradeLoetelu { get; set; }
        public Option<boolean> VarasemKaristatus { get; set; }
        public Option<string> VarasemKaristatusKirjeldus { get; set; }

        public class VastutatavadKohustisedType : IXRoadXmlSerializable
        {
            public Option<Kohustis> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<VastutatavadKohustisedType> VastutatavadKohustised { get; set; }
        public Option<DateTime> VerAlgusKP { get; set; }
        public Option<DateTime> VerLoppKP { get; set; }
        public Option<long> VersID { get; set; }

        public class VoladType : IXRoadXmlSerializable
        {
            public Option<OsaNoue> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<VoladType> Volad { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}