using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class OsaliseSeotudOsalineSeoseHaldamineRequest : IXRoadXmlSerializable
    {
        public Osaline Osaline { get; set; }
        public Option<Osaline> MaaratudEsindaja { get; set; }
        public Option<Osaline> AsendatavEsindaja { get; set; }
        public Option<Menetlus> Menetlus { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}