using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MaaraMenetlejaRequest : IXRoadXmlSerializable
    {
        public Option<Osaline> MaaratudMenetleja { get; set; }
        public Option<Osaline> AsendatavMenetleja { get; set; }
        public Menetlus Menetlus { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}