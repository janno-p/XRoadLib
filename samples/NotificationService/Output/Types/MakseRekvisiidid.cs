using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MakseRekvisiidid : Konto
    {
        public Option<string> MakseSaaja { get; set; }
        public Option<string> MakseSaajaKood { get; set; }
        public Option<string> MakseSelgitus { get; set; }
        public Option<string> MakseValisriiki { get; set; }
        public Option<string> ViiteNumber { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}