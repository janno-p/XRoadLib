using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Syyteosyndmus : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<IList<Toiming>> Avaldused { get; set; }
        public Option<string> Faabula { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<IList<long>> KahtlTookohtadToimepAjalKL { get; set; }
        public Option<string> KannatanuCSV { get; set; }
        public Option<IList<long>> KannatanuJooveKL { get; set; }
        public Option<long?> KannatanuSuheKahtlvSyydistKL { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<IList<long>> KohaliikKL { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<IList<KvalifikatsiooniParagrahv>> Kvalifikatsioon { get; set; }
        public Option<string> KvalifikatsioonCSV { get; set; }
        public Option<IList<long>> LiigitusTunnusedKL { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<IList<Menetlus>> Menetlused { get; set; }
        public Option<long?> MotiivKL { get; set; }
        public Option<int?> MuuTervisekahjustuseSaanuteArv { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<IList<Objekt>> Objektid { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> ObjektideLoetelu { get; set; }
        public Option<IList<Osaline>> Osalised { get; set; }
        public Option<decimal?> RahaliseKahjuSuurus { get; set; }
        public Option<long?> RahaliseKahjuValuutaKL { get; set; }
        public Option<int?> RaskeTervisekahjustuseSaanuteArv { get; set; }
        public Option<IList<long>> RelvaliikJaKasutamineKL { get; set; }
        public Option<IList<KvalifikatsiooniParagrahv>> RikutudOigusnorm { get; set; }
        public Option<string> RikutudOigusnormCSV { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<int?> SurmaSaanuteArv { get; set; }
        public Option<string> SyystatavCSV { get; set; }
        public Option<string> SyyteoNR { get; set; }
        public Option<long> SyyteoObjektID { get; set; }
        public Option<IList<long>> TaideviijaJooveKL { get; set; }
        public Option<DateTime?> ToimumiseAlgusKP { get; set; }
        public Option<Aadress> ToimumiseKoht { get; set; }
        public Option<string> ToimumiseKohtCSV { get; set; }
        public Option<DateTime?> ToimumiseLoppKP { get; set; }
        public Option<string> ToimumisKohaKirjeldus { get; set; }
        public Option<long?> VagivaldKL { get; set; }
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