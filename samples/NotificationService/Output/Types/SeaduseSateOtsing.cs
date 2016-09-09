using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SeaduseSateOtsing : IXRoadXmlSerializable
    {
        public Option<boolean> EXCLUDE { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<string> Loige { get; set; }
        public Option<string> LoigePrimm { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> Paragrahv { get; set; }
        public Option<string> ParagrahvPrimm { get; set; }
        public Option<string> Punkt { get; set; }
        public Option<string> PunktPrimm { get; set; }
        public Option<long> SeadustikKL { get; set; }
        public Option<string> Tekst { get; set; }
        public Option<string> TekstLyhendatult { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}