using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguTagastamineKustutamineRequest : IXRoadXmlSerializable
    {
        public Toiming Toiming { get; set; }
        public KLVaartus StaatusKL { get; set; }
        public Isik Kasutaja { get; set; }
        public Option<boolean> RakendaLisadele { get; set; }
        public Option<string> Selgitus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}