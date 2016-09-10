using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaMenetlusedRequest : IXRoadXmlSerializable
    {
        public IList<MenetlusOtsing> Menetlused { get; set; }
        public IList<OsalineOtsing> Osalised { get; set; }
        public IList<ToimingOtsing> Toimingud { get; set; }
        public IList<SyyteosyndmusOtsing> Syyteosyndmused { get; set; }
        public Option<bool> OtsiArhiveerimataMenetlusi { get; set; }
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