using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Toiming : IXRoadXmlSerializable
    {
        public Option<long> AlaLiigiTapsustusKL { get; set; }
        public Option<long> AlaLiikKL { get; set; }
        public Option<DateTime> AlgusKP { get; set; }
        public Option<long> AllikaksOlevKlientSysteemKL { get; set; }

        public class AlusType : IXRoadXmlSerializable
        {
            public Option<SeaduseSate> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlusType> Alus { get; set; }

        public class AluseTapsustusKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AluseTapsustusKLType> AluseTapsustusKL { get; set; }
        public Option<string> Alustaja { get; set; }

        public class AlusToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlusToimingudType> AlusToimingud { get; set; }
        public Option<string> AsjaajamiseNumber { get; set; }

        public class AvalikustatudFailidType : IXRoadXmlSerializable
        {
            public Option<Fail> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AvalikustatudFailidType> AvalikustatudFailid { get; set; }
        public Option<DateTime> DokumendiPostitamiseKP { get; set; }
        public Option<DateTime> DokumendiSaabumiseKP { get; set; }
        public Option<string> EcliNR { get; set; }
        public Option<boolean> EdasiKaebusOigusestLoobutud { get; set; }
        public Option<string> EdasilykkamatusePohjendus { get; set; }
        public Option<boolean> EdastadaRaamatupidamisse { get; set; }
        public Option<long> EdastamiseViisKL { get; set; }
        public Option<string> Faabula { get; set; }

        public class FailidType : IXRoadXmlSerializable
        {
            public Option<Fail> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<FailidType> Failid { get; set; }
        public Option<boolean> HagiTagamineEsialgneOiguskaitse { get; set; }

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
        public Option<long> JarjekorraNR { get; set; }
        public Option<string> KlientsysteemiID { get; set; }

        public class KobaraLiikmedType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KobaraLiikmedType> KobaraLiikmed { get; set; }
        public Option<long> KobaraTunnus { get; set; }
        public Option<string> KoostamiseKoht { get; set; }
        public Option<DateTime> KoostamiseKP { get; set; }
        public Option<long> KorduvuseMargeKL { get; set; }
        public Option<string> Korraldused { get; set; }

        public class KuludType : IXRoadXmlSerializable
        {
            public Option<Kulu> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KuludType> Kulud { get; set; }
        public Option<DateTime> LabiviimiseAlgusKP { get; set; }
        public Option<DateTime> LabiviimiseLoppKP { get; set; }
        public Option<long> LabiviimiseVormKL { get; set; }
        public Option<DateTime> LahendiKuulutamiseAeg { get; set; }
        public Option<long> LahenduseLiikKL { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<string> Lisad { get; set; }

        public class LisaToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LisaToimingudType> LisaToimingud { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime> LoppKP { get; set; }

        public class MakseRekvisiididType : IXRoadXmlSerializable
        {
            public Option<MakseRekvisiidid> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MakseRekvisiididType> MakseRekvisiidid { get; set; }
        public Option<long> MarksonastatusKL { get; set; }
        public Option<string> Markused { get; set; }
        public Option<Menetlus> Menetlus { get; set; }

        public class MenetlusPostType : IXRoadXmlSerializable
        {
            public Option<MenetlusPost> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusPostType> MenetlusPost { get; set; }
        public Option<long> MojutatavaToiminguLahenduseTapsustusKL { get; set; }
        public Option<long> MojutatavaToiminguLahendusKL { get; set; }

        public class MojutavadToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MojutavadToimingudType> MojutavadToimingud { get; set; }
        public Option<boolean> MuuJuurdepaasupiirang { get; set; }
        public Option<string> MuuJuurdepaasupiiranguPohjendus { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime> MuutmiseKP { get; set; }
        public Option<string> Nimetus { get; set; }

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
        public Option<boolean> OigusabiMenetlusabiSaamine { get; set; }
        public Option<string> OlustikuKirjeldus { get; set; }
        public Option<long> PiirkondKL { get; set; }
        public Option<Fail> PohiFail { get; set; }

        public class PohiToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<PohiToimingudType> PohiToimingud { get; set; }
        public Option<string> Pohjendus { get; set; }
        public Option<string> Resolutsioon { get; set; }

        public class RiigiOigusabiType : IXRoadXmlSerializable
        {
            public Option<RiigiOigusabi> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<RiigiOigusabiType> RiigiOigusabi { get; set; }
        public Option<long> RiigioigusabiAndmiseViisKL { get; set; }
        public Option<long> RoaLiikKL { get; set; }
        public Option<DateTime> SalastatuseAlgusKP { get; set; }
        public Option<string> SalastatuseAlusKirjeldus { get; set; }
        public Option<long> SalastatuseAlusKL { get; set; }
        public Option<DateTime> SalastatuseLoppKP { get; set; }
        public Option<long> SalastatuseTaseKL { get; set; }

        public class SeisundidKLType : IXRoadXmlSerializable
        {
            public Option<Olek> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeisundidKLType> SeisundidKL { get; set; }
        public Option<long> SeisundKL { get; set; }
        public Option<DateTime> SeisundKP { get; set; }

        public class SeotudToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeotudToimingudType> SeotudToimingud { get; set; }
        public Option<string> SihtMenetluseNR { get; set; }
        public Option<long> SihtMenetluseObjektID { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime> SisestamiseKP { get; set; }
        public Option<string> Sissejuhatus { get; set; }
        public Option<long> SissenoutavusKL { get; set; }
        public Option<string> Sisu { get; set; }

        public class SisulisedLahendusedType : IXRoadXmlSerializable
        {
            public Option<MenetluseSisulineLiigitus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SisulisedLahendusedType> SisulisedLahendused { get; set; }

        public class StaatusedKLType : IXRoadXmlSerializable
        {
            public Option<Olek> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<StaatusedKLType> StaatusedKL { get; set; }

        public class StaatuseMargeKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<StaatuseMargeKLType> StaatuseMargeKL { get; set; }
        public Option<long> StaatusKL { get; set; }
        public Option<DateTime> StaatusKP { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }

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

        public class TagasimaksedType : IXRoadXmlSerializable
        {
            public Option<Tagasimakse> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TagasimaksedType> Tagasimaksed { get; set; }
        public Option<DateTime> TahtaegKP { get; set; }
        public Option<string> TeavitamiseKirjeldus { get; set; }

        public class TegevusekohadType : IXRoadXmlSerializable
        {
            public Option<Aadress> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TegevusekohadType> Tegevusekohad { get; set; }
        public Option<string> TehnikavahenditeLoetelu { get; set; }
        public Option<decimal> ToimingugaSeotudSumma { get; set; }
        public Option<long> ToimingugaSeotudSummaValuutaKL { get; set; }

        public class ToiminguMenetlejadType : IXRoadXmlSerializable
        {
            public Option<ToiminguOsaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToiminguMenetlejadType> ToiminguMenetlejad { get; set; }
        public Option<string> ToiminguMenetlejadCSV { get; set; }
        public Option<string> ToiminguNR { get; set; }

        public class ToiminguosalisedType : IXRoadXmlSerializable
        {
            public Option<ToiminguOsaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToiminguosalisedType> Toiminguosalised { get; set; }
        public Option<string> ToiminguosalisedCSV { get; set; }

        public class ToiminguosaliseKontaktidType : IXRoadXmlSerializable
        {
            public Option<Kontakt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToiminguosaliseKontaktidType> ToiminguosaliseKontaktid { get; set; }
        public Option<string> ToiminguVastutavKontakt { get; set; }
        public Option<JuriidilineIsik> UusMenetlevAsutus { get; set; }
        public Option<DateTime> VerAlgusKP { get; set; }
        public Option<DateTime> VerLoppKP { get; set; }
        public Option<long> VersID { get; set; }
        public Option<DateTime> ViimaseSalvestamiseAeg { get; set; }
        public Option<boolean> VoibMakstaPangalingiga { get; set; }

        public class YhendatavadEraldatavadAsjadType : IXRoadXmlSerializable
        {
            public Option<Menetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<YhendatavadEraldatavadAsjadType> YhendatavadEraldatavadAsjad { get; set; }
        public Option<string> YhendatavadEraldatavadAsjadCSV { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}