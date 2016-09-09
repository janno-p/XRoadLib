using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SeostaOigusaktidRequest : IXRoadXmlSerializable
    {
        public long ToiminguObjektID { get; set; }

        public class OigusaktidType : IXRoadXmlSerializable
        {
            public Option<Oigusakt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OigusaktidType> Oigusaktid { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}