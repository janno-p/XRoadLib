using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetluseTeavitus : StandardTeavitus, IXRoadXmlSerializable
    {
        public Option<long> AsjaLiikKL { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<long> AsjaObjektID { get; set; }
        public Option<Menetlus> Menetlus { get; set; }
        public Option<string> MenetluseMenetlevadAsutusedCSV { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<long> MenetluseObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}