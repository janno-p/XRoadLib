using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KOLASeaduseSate : IXRoadXmlSerializable
    {
        public Option<string> Loige { get; set; }
        public Option<int?> ParagLiik { get; set; }
        public Option<string> Paragrahv { get; set; }
        public Option<string> Punkt { get; set; }
        public Option<int?> Seadustik { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}