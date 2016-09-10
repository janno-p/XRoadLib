using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SyyteosyndmusOtsing : IXRoadXmlSerializable
    {
        public Option<AadressOtsing> Aadress { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<DateTime?> AlgusKPVahemikuLoppKP { get; set; }
        public Option<bool> EXCLUDE { get; set; }
        public Option<string> Faabula { get; set; }
        public Option<bool?> KahtlSyydistOnAlaealine { get; set; }
        public Option<IList<long>> KahtlTookohtadToimepAjalKL { get; set; }
        public Option<bool?> KannatanuOnAlaealine { get; set; }
        public Option<IList<long>> KannatanuSuheKahtlvSyydistKL { get; set; }
        public Option<IList<long>> KohaliikKL { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<DateTime?> KoostamiseKPVahemikuLoppKP { get; set; }
        public Option<IList<KvalifikatsiooniParagrahvOtsing>> Kvalifikatsioon { get; set; }
        public Option<KvalifikatsiooniParagrahvOtsing> KvalifikatsiooniVahemikuLopp { get; set; }
        public Option<IList<long>> LiigitusTunnusedKL { get; set; }
        public Option<decimal?> RahaliseKahjuSuurusAlates { get; set; }
        public Option<decimal?> RahaliseKahjuSuurusKuni { get; set; }
        public Option<long?> RahaliseKahjuValuutaKL { get; set; }
        public Option<string> SyyteoNR { get; set; }
        public Option<DateTime?> ToimumisVahemikuAlgusKP { get; set; }
        public Option<DateTime?> ToimumisVahemikuLoppKP { get; set; }
        public Option<long?> VagivaldKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}