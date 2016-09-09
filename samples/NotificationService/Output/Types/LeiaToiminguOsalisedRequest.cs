using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaToiminguOsalisedRequest : IXRoadXmlSerializable
    {
        public class ToiminguOsalisedType : IXRoadXmlSerializable
        {
            public Option<ToiminguOsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public ToiminguOsalisedType ToiminguOsalised { get; set; }
        public Isik Kasutaja { get; set; }
        public Option<OtsinguMeta> OtsinguMeta { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}