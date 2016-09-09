using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Istungisaal : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<JuriidilineIsik> Kohtumaja { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<long> ObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}