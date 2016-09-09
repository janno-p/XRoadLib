using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class AsiOtsing : IXRoadXmlSerializable
    {
        public Option<DateTime> AsjaAlgusKP { get; set; }
        public Option<DateTime> AsjaAlgusKPVahemikuLoppKP { get; set; }

        public class AsjaLiikKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AsjaLiikKLType> AsjaLiikKL { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<long> AsjaObjektID { get; set; }
        public Option<long> AsjaSeisundKL { get; set; }

        public class ByrooOsalisedType : IXRoadXmlSerializable
        {
            public Option<OsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ByrooOsalisedType> ByrooOsalised { get; set; }
        public Option<boolean> EXCLUDE { get; set; }

        public class MenetlusedType : IXRoadXmlSerializable
        {
            public Option<MenetlusOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusedType> Menetlused { get; set; }

        public class OsalisedType : IXRoadXmlSerializable
        {
            public Option<OsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsalisedType> Osalised { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}