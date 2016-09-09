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
        public Option<boolean> EXCLUDE { get; set; }
        public Option<string> Faabula { get; set; }
        public Option<boolean> KahtlSyydistOnAlaealine { get; set; }

        public class KahtlTookohtadToimepAjalKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KahtlTookohtadToimepAjalKLType> KahtlTookohtadToimepAjalKL { get; set; }
        public Option<boolean> KannatanuOnAlaealine { get; set; }

        public class KannatanuSuheKahtlvSyydistKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KannatanuSuheKahtlvSyydistKLType> KannatanuSuheKahtlvSyydistKL { get; set; }

        public class KohaliikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KohaliikKLType> KohaliikKL { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<DateTime?> KoostamiseKPVahemikuLoppKP { get; set; }

        public class KvalifikatsioonType : IXRoadXmlSerializable
        {
            public IList<KvalifikatsiooniParagrahvOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KvalifikatsioonType> Kvalifikatsioon { get; set; }
        public Option<KvalifikatsiooniParagrahvOtsing> KvalifikatsiooniVahemikuLopp { get; set; }

        public class LiigitusTunnusedKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LiigitusTunnusedKLType> LiigitusTunnusedKL { get; set; }
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