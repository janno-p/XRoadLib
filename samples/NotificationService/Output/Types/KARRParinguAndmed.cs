using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KARRParinguAndmed : IXRoadXmlSerializable
    {
        public Option<string> AvalduseNR { get; set; }
        public Option<long?> EesmarkKL { get; set; }
        public Option<string> Infosysteem { get; set; }
        public Option<string> Kommentaar { get; set; }
        public Option<DateTime?> ParinguTegemiseValitudAeg { get; set; }
        public Option<boolean> Salastatud { get; set; }
        public Option<boolean> Tasuline { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}