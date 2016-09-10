using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaToimingudJaLisadKISRequest : IXRoadXmlSerializable
    {
        public IList<ToimingOtsing> Toimingud { get; set; }
        public Isik Kasutaja { get; set; }
        public Option<OtsinguMeta> OtsinguMeta { get; set; }
        public bool TagastaToiminguLisad { get; set; }
        public bool TagastaPohiToimingud { get; set; }
        public bool TagastaSeotudToimingud { get; set; }
        public bool TagastaAlusToimingud { get; set; }
        public bool TagastaAnnotatsioonid { get; set; }
        public bool TagastaMojutatavadToimingud { get; set; }
        public bool TagastaMojutavadToimingud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}