using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class OsalineOtsing : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<DateTime?> AlgusKPVahemikuLoppKP { get; set; }
        public Option<IList<ByrooPiirang>> ByrooPiirangud { get; set; }
        public Option<bool> EXCLUDE { get; set; }
        public Option<string> IsikuEesnimi { get; set; }
        public Option<string> IsikuKood { get; set; }
        public Option<string> IsikuKoosnimi { get; set; }
        public Option<string> IsikuMuudeRiikideKoodid { get; set; }
        public Option<string> IsikuNimi { get; set; }
        public Option<long?> IsikuObjektID { get; set; }
        public Option<DateTime?> IsikuSynniKP { get; set; }
        public Option<DateTime?> IsikuSynniKPVahemikuLoppKP { get; set; }
        public Option<IList<long>> IsikuTyypKL { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<DateTime?> KoostamiseKPVahemikuLoppKP { get; set; }
        public Option<IList<KvalifikatsiooniParagrahvOtsing>> Kvalifikatsioon { get; set; }
        public Option<string> KvalifikatsioonCSV { get; set; }
        public Option<KvalifikatsiooniParagrahvOtsing> KvalifikatsiooniVahemikuLopp { get; set; }
        public Option<IList<long>> LiikKL { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<bool?> LoppKPIsNull { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<JuriidilineIsikOtsing> Organisatsiooniyksus { get; set; }
        public Option<IList<OsalineOtsing>> OsaliseEsindaja { get; set; }
        public Option<IList<OsalineOtsing>> OsaliseEsindatav { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}