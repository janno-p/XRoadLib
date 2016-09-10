using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Osaline : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<IList<Osaline>> Esindajad { get; set; }
        public Option<IList<Osaline>> Esindatavad { get; set; }
        public Option<string> Haigestumine { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<Isik> Isik { get; set; }
        public Option<string> IsikCSV { get; set; }
        public Option<string> IsikuEritunnusteKirjeldus { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<IList<KvalifikatsiooniParagrahv>> Kvalifikatsioon { get; set; }
        public Option<string> KvalifikatsioonCSV { get; set; }
        public Option<string> Leppenimi { get; set; }
        public Option<long?> LiikAsjasKL { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<string> Markused { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<IList<Noue>> Nouded { get; set; }
        public Option<IList<Objekt>> Objektid { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> ObjektideLoetelu { get; set; }
        public Option<string> Ohtlikkus { get; set; }
        public Option<string> OsalistIseloomustavadAndmed { get; set; }
        public Option<decimal?> PaevaSissetulek { get; set; }
        public Option<DateTime?> PaevaSissetulekKP { get; set; }
        public Option<long?> PaevaSissetulekValuutaKL { get; set; }
        public Option<bool?> PoleMenetlussePuutuv { get; set; }
        public Option<IList<KvalifikatsiooniParagrahv>> RikutudOigusnorm { get; set; }
        public Option<string> RikutudOigusnormCSV { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }
        public Option<IList<Sanktsioon>> Sanktsioonid { get; set; }
        public Option<IList<Menetlus>> SeotudAsjad { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<string> SundraviVajadus { get; set; }
        public Option<string> SyydIseloomustab { get; set; }
        public Option<string> Syydistus { get; set; }
        public Option<IList<SyydistusPunkt>> SyydistusPunktid { get; set; }
        public Option<IList<Toiming>> SyydToendavadToimingud { get; set; }
        public Option<string> SyydToendavateToiminguteLoetelu { get; set; }
        public Option<IList<Syyteosyndmus>> Syyteosyndmused { get; set; }
        public Option<string> ToimikuLkNR { get; set; }
        public Option<IList<Toiming>> Toimingud { get; set; }
        public Option<string> VaimneSeisund { get; set; }
        public Option<IList<Objekt>> Varad { get; set; }
        public Option<string> VaradeLoetelu { get; set; }
        public Option<bool?> VarasemKaristatus { get; set; }
        public Option<string> VarasemKaristatusKirjeldus { get; set; }
        public Option<IList<Kohustis>> VastutatavadKohustised { get; set; }
        public Option<DateTime?> VerAlgusKP { get; set; }
        public Option<DateTime?> VerLoppKP { get; set; }
        public Option<long> VersID { get; set; }
        public Option<IList<OsaNoue>> Volad { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}