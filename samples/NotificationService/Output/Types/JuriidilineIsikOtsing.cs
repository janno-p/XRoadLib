using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class JuriidilineIsikOtsing : IsikOtsing
    {
        public Option<boolean> KaristatudIsik { get; set; }

        public class KontaktidType : IXRoadXmlSerializable
        {
            public Option<KontaktOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KontaktidType> Kontaktid { get; set; }
        public Option<boolean> OnKehtiv { get; set; }
        public Option<boolean> OnSysteemne { get; set; }
        public Option<boolean> OtsiIlmaAlamasutusteta { get; set; }
        public Option<DateTime> TegevusAlgusKP { get; set; }
        public Option<DateTime> TegevusLoppKP { get; set; }

        public class VormKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<VormKLType> VormKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}