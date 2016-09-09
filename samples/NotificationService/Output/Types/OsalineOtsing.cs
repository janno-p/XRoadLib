using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class OsalineOtsing : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<DateTime> AlgusKPVahemikuLoppKP { get; set; }

        public class ByrooPiirangudType : IXRoadXmlSerializable
        {
            public Option<ByrooPiirang> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ByrooPiirangudType> ByrooPiirangud { get; set; }
        public Option<boolean> EXCLUDE { get; set; }
        public Option<string> IsikuEesnimi { get; set; }
        public Option<string> IsikuKood { get; set; }
        public Option<string> IsikuKoosnimi { get; set; }
        public Option<string> IsikuMuudeRiikideKoodid { get; set; }
        public Option<string> IsikuNimi { get; set; }
        public Option<long> IsikuObjektID { get; set; }
        public Option<DateTime> IsikuSynniKP { get; set; }
        public Option<DateTime> IsikuSynniKPVahemikuLoppKP { get; set; }

        public class IsikuTyypKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<IsikuTyypKLType> IsikuTyypKL { get; set; }
        public Option<DateTime> KoostamiseKP { get; set; }
        public Option<DateTime> KoostamiseKPVahemikuLoppKP { get; set; }

        public class KvalifikatsioonType : IXRoadXmlSerializable
        {
            public Option<KvalifikatsiooniParagrahvOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KvalifikatsioonType> Kvalifikatsioon { get; set; }
        public Option<string> KvalifikatsioonCSV { get; set; }
        public Option<KvalifikatsiooniParagrahvOtsing> KvalifikatsiooniVahemikuLopp { get; set; }

        public class LiikKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LiikKLType> LiikKL { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<boolean> LoppKPIsNull { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<JuriidilineIsikOtsing> Organisatsiooniyksus { get; set; }

        public class OsaliseEsindajaType : IXRoadXmlSerializable
        {
            public Option<OsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsaliseEsindajaType> OsaliseEsindaja { get; set; }

        public class OsaliseEsindatavType : IXRoadXmlSerializable
        {
            public Option<OsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsaliseEsindatavType> OsaliseEsindatav { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}