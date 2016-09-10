using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Annotatsioon : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<IList<Marksona>> Marksonad { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<string> Sisu { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}