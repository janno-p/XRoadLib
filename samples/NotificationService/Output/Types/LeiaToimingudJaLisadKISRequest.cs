using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaToimingudJaLisadKISRequest : IXRoadXmlSerializable
    {
        public class ToimingudType : IXRoadXmlSerializable
        {
            public Option<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public ToimingudType Toimingud { get; set; }
        public Isik Kasutaja { get; set; }
        public Option<OtsinguMeta> OtsinguMeta { get; set; }
        public boolean TagastaToiminguLisad { get; set; }
        public boolean TagastaPohiToimingud { get; set; }
        public boolean TagastaSeotudToimingud { get; set; }
        public boolean TagastaAlusToimingud { get; set; }
        public boolean TagastaAnnotatsioonid { get; set; }
        public boolean TagastaMojutatavadToimingud { get; set; }
        public boolean TagastaMojutavadToimingud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}