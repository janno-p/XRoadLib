using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KohustisOtsing : IXRoadXmlSerializable
    {
        public Option<IList<long>> AlaLiikKL { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<DateTime?> AlgusKPVahemikuLoppKP { get; set; }
        public Option<IList<long>> GruppKL { get; set; }
        public Option<bool> IsikudSolidaarselt { get; set; }
        public Option<string> IsikuEesnimi { get; set; }
        public Option<string> IsikuKood { get; set; }
        public Option<string> IsikuKoosnimi { get; set; }
        public Option<string> IsikuNimi { get; set; }
        public Option<long?> IsikuObjektID { get; set; }
        public Option<int> IsikuVanusMaaramisel { get; set; }
        public Option<int> IsikuVanusMaaramiselVahemikuLoppVanus { get; set; }
        public Option<IList<long>> LiikKL { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<bool?> LoppKPIsNull { get; set; }
        public Option<IList<MakseRekvisiidid>> MakseRekvisiidid { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<long> MenetluseObjektID { get; set; }
        public Option<decimal?> MoistetudSumma { get; set; }
        public Option<long?> MoistetudSummaValuutaKL { get; set; }
        public Option<long?> NoueObjektID { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<long?> OsaliseObjektID { get; set; }
        public Option<DateTime?> RakendamiseTahtaegKP { get; set; }
        public Option<DateTime?> RakendamiseTahtaegKPVahemikuLoppKP { get; set; }
        public Option<DateTime?> RakendumiseAlgusKP { get; set; }
        public Option<DateTime?> RakendumiseAlgusKPVahemikuLoppKP { get; set; }
        public Option<DateTime?> RakendumiseLoppKP { get; set; }
        public Option<bool?> RakendumiseLoppKPIsNull { get; set; }
        public Option<DateTime?> RakendumiseLoppKPVahemikuLoppKP { get; set; }
        public Option<IList<long>> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<bool?> Taidetud { get; set; }
        public Option<long?> TaitmiseTapsustusKL { get; set; }
        public Option<decimal?> TasutudSumma { get; set; }
        public Option<long?> TasutudSummaValuutaKL { get; set; }
        public Option<IList<long>> TyypKL { get; set; }
        public Option<bool?> VoibKandaOsiti { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}