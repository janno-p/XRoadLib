using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Toiming : IXRoadXmlSerializable
    {
        public Option<long?> AlaLiigiTapsustusKL { get; set; }
        public Option<long?> AlaLiikKL { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<long?> AllikaksOlevKlientSysteemKL { get; set; }
        public Option<IList<SeaduseSate>> Alus { get; set; }
        public Option<IList<long>> AluseTapsustusKL { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<IList<Toiming>> AlusToimingud { get; set; }
        public Option<string> AsjaajamiseNumber { get; set; }
        public Option<IList<Fail>> AvalikustatudFailid { get; set; }
        public Option<DateTime?> DokumendiPostitamiseKP { get; set; }
        public Option<DateTime?> DokumendiSaabumiseKP { get; set; }
        public Option<string> EcliNR { get; set; }
        public Option<bool?> EdasiKaebusOigusestLoobutud { get; set; }
        public Option<string> EdasilykkamatusePohjendus { get; set; }
        public Option<bool?> EdastadaRaamatupidamisse { get; set; }
        public Option<long?> EdastamiseViisKL { get; set; }
        public Option<string> Faabula { get; set; }
        public Option<IList<Fail>> Failid { get; set; }
        public Option<bool?> HagiTagamineEsialgneOiguskaitse { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<long?> JarjekorraNR { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<IList<Toiming>> KobaraLiikmed { get; set; }
        public Option<long?> KobaraTunnus { get; set; }
        public Option<string> KoostamiseKoht { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<long?> KorduvuseMargeKL { get; set; }
        public Option<string> Korraldused { get; set; }
        public Option<IList<Kulu>> Kulud { get; set; }
        public Option<DateTime?> LabiviimiseAlgusKP { get; set; }
        public Option<DateTime?> LabiviimiseLoppKP { get; set; }
        public Option<long?> LabiviimiseVormKL { get; set; }
        public Option<DateTime?> LahendiKuulutamiseAeg { get; set; }
        public Option<long?> LahenduseLiikKL { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<string> Lisad { get; set; }
        public Option<IList<Toiming>> LisaToimingud { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<IList<MakseRekvisiidid>> MakseRekvisiidid { get; set; }
        public Option<long?> MarksonastatusKL { get; set; }
        public Option<string> Markused { get; set; }
        public Option<Menetlus> Menetlus { get; set; }
        public Option<IList<MenetlusPost>> MenetlusPost { get; set; }
        public Option<long?> MojutatavaToiminguLahenduseTapsustusKL { get; set; }
        public Option<long?> MojutatavaToiminguLahendusKL { get; set; }
        public Option<IList<Toiming>> MojutavadToimingud { get; set; }
        public Option<bool?> MuuJuurdepaasupiirang { get; set; }
        public Option<string> MuuJuurdepaasupiiranguPohjendus { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<IList<Noue>> Nouded { get; set; }
        public Option<IList<Objekt>> Objektid { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> ObjektideLoetelu { get; set; }
        public Option<bool?> OigusabiMenetlusabiSaamine { get; set; }
        public Option<string> OlustikuKirjeldus { get; set; }
        public Option<long?> PiirkondKL { get; set; }
        public Option<Fail> PohiFail { get; set; }
        public Option<IList<Toiming>> PohiToimingud { get; set; }
        public Option<string> Pohjendus { get; set; }
        public Option<string> Resolutsioon { get; set; }
        public Option<IList<RiigiOigusabi>> RiigiOigusabi { get; set; }
        public Option<long?> RiigioigusabiAndmiseViisKL { get; set; }
        public Option<long?> RoaLiikKL { get; set; }
        public Option<DateTime?> SalastatuseAlgusKP { get; set; }
        public Option<string> SalastatuseAlusKirjeldus { get; set; }
        public Option<long?> SalastatuseAlusKL { get; set; }
        public Option<DateTime?> SalastatuseLoppKP { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }
        public Option<IList<Olek>> SeisundidKL { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<IList<Toiming>> SeotudToimingud { get; set; }
        public Option<string> SihtMenetluseNR { get; set; }
        public Option<long> SihtMenetluseObjektID { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<string> Sissejuhatus { get; set; }
        public Option<long?> SissenoutavusKL { get; set; }
        public Option<string> Sisu { get; set; }
        public Option<IList<MenetluseSisulineLiigitus>> SisulisedLahendused { get; set; }
        public Option<IList<Olek>> StaatusedKL { get; set; }
        public Option<IList<long>> StaatuseMargeKL { get; set; }
        public Option<long?> StaatusKL { get; set; }
        public Option<DateTime?> StaatusKP { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<IList<Syyteosyndmus>> Syyteosyndmused { get; set; }
        public Option<IList<Tagasimakse>> Tagasimaksed { get; set; }
        public Option<DateTime?> TahtaegKP { get; set; }
        public Option<string> TeavitamiseKirjeldus { get; set; }
        public Option<IList<Aadress>> Tegevusekohad { get; set; }
        public Option<string> TehnikavahenditeLoetelu { get; set; }
        public Option<decimal?> ToimingugaSeotudSumma { get; set; }
        public Option<long?> ToimingugaSeotudSummaValuutaKL { get; set; }
        public Option<IList<ToiminguOsaline>> ToiminguMenetlejad { get; set; }
        public Option<string> ToiminguMenetlejadCSV { get; set; }
        public Option<string> ToiminguNR { get; set; }
        public Option<IList<ToiminguOsaline>> Toiminguosalised { get; set; }
        public Option<string> ToiminguosalisedCSV { get; set; }
        public Option<IList<Kontakt>> ToiminguosaliseKontaktid { get; set; }
        public Option<string> ToiminguVastutavKontakt { get; set; }
        public Option<JuriidilineIsik> UusMenetlevAsutus { get; set; }
        public Option<DateTime?> VerAlgusKP { get; set; }
        public Option<DateTime?> VerLoppKP { get; set; }
        public Option<long> VersID { get; set; }
        public Option<DateTime?> ViimaseSalvestamiseAeg { get; set; }
        public Option<bool?> VoibMakstaPangalingiga { get; set; }
        public Option<IList<Menetlus>> YhendatavadEraldatavadAsjad { get; set; }
        public Option<string> YhendatavadEraldatavadAsjadCSV { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}