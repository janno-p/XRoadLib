using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class LeiaMaksekorraldusedRequest : IXRoadXmlSerializable
    {
        public string MaksekorralduseNR { get; set; }
        public string MaksjaNimi { get; set; }
        public DateTime? MakseVahemikuAlgusKP { get; set; }
        public DateTime? MakseVahemikuLoppKP { get; set; }
        public long? MakseViisKL { get; set; }
        public long? PankKL { get; set; }
        public string Viitenumber { get; set; }
        public string Selgitus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}