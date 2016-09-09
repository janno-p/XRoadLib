using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Syyteosyndmus : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }

        public class AvaldusedType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AvaldusedType> Avaldused { get; set; }
        public Option<string> Faabula { get; set; }

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

        public class KahtlTookohtadToimepAjalKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KahtlTookohtadToimepAjalKLType> KahtlTookohtadToimepAjalKL { get; set; }
        public Option<string> KannatanuCSV { get; set; }

        public class KannatanuJooveKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KannatanuJooveKLType> KannatanuJooveKL { get; set; }
        public Option<long> KannatanuSuheKahtlvSyydistKL { get; set; }
        public Option<string> KlientsysteemiID { get; set; }

        public class KohaliikKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KohaliikKLType> KohaliikKL { get; set; }
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

        public class LiigitusTunnusedKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LiigitusTunnusedKLType> LiigitusTunnusedKL { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime> LoppKP { get; set; }

        public class MenetlusedType : IXRoadXmlSerializable
        {
            public Option<Menetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusedType> Menetlused { get; set; }
        public Option<long> MotiivKL { get; set; }
        public Option<int> MuuTervisekahjustuseSaanuteArv { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime> MuutmiseKP { get; set; }

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

        public class OsalisedType : IXRoadXmlSerializable
        {
            public Option<Osaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsalisedType> Osalised { get; set; }
        public Option<decimal> RahaliseKahjuSuurus { get; set; }
        public Option<long> RahaliseKahjuValuutaKL { get; set; }
        public Option<int> RaskeTervisekahjustuseSaanuteArv { get; set; }

        public class RelvaliikJaKasutamineKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<RelvaliikJaKasutamineKLType> RelvaliikJaKasutamineKL { get; set; }

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
        public Option<long> SeisundKL { get; set; }
        public Option<DateTime> SeisundKP { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime> SisestamiseKP { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }
        public Option<int> SurmaSaanuteArv { get; set; }
        public Option<string> SyystatavCSV { get; set; }
        public Option<string> SyyteoNR { get; set; }
        public Option<long> SyyteoObjektID { get; set; }

        public class TaideviijaJooveKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TaideviijaJooveKLType> TaideviijaJooveKL { get; set; }
        public Option<DateTime> ToimumiseAlgusKP { get; set; }
        public Option<Aadress> ToimumiseKoht { get; set; }
        public Option<string> ToimumiseKohtCSV { get; set; }
        public Option<DateTime> ToimumiseLoppKP { get; set; }
        public Option<string> ToimumisKohaKirjeldus { get; set; }
        public Option<long> VagivaldKL { get; set; }
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