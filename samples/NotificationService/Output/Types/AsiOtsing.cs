using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class AsiOtsing : IXRoadXmlSerializable
    {
        public Option<DateTime?> AsjaAlgusKP { get; set; }
        public Option<DateTime?> AsjaAlgusKPVahemikuLoppKP { get; set; }
        public Option<IList<long>> AsjaLiikKL { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<long?> AsjaObjektID { get; set; }
        public Option<long?> AsjaSeisundKL { get; set; }
        public Option<IList<OsalineOtsing>> ByrooOsalised { get; set; }
        public Option<bool> EXCLUDE { get; set; }
        public Option<IList<MenetlusOtsing>> Menetlused { get; set; }
        public Option<IList<OsalineOtsing>> Osalised { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}