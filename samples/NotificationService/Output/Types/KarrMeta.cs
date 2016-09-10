using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrMeta : IXRoadXmlSerializable
    {
        public Option<long?> AsjaLiikKL { get; set; }
        public Option<string> AvalduseNR { get; set; }
        public Option<string> Infosysteem { get; set; }
        public Option<bool?> OnSalastatud { get; set; }
        public Option<bool?> OnTasuline { get; set; }
        public Option<bool?> OtsiArhiivist { get; set; }
        public Option<long?> ParinguEesmarkKL { get; set; }
        public Option<int?> ParinguID { get; set; }
        public Option<bool?> PiiratudParing { get; set; }
        public Option<string> Pohjendus { get; set; }
        public Option<DateTime?> VahemikuAlgusKP { get; set; }
        public Option<DateTime?> VahemikuLoppKP { get; set; }
        public Option<DateTime?> ValitudParinguTegemiseAeg { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}