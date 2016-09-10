using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SeotudMenetlus : Menetlus, IXRoadXmlSerializable
    {
        public Option<string> MenetlejadCSV { get; set; }
        public Option<bool?> Peaasi { get; set; }
        public Option<long?> SidumiseAlusKL { get; set; }
        public Option<DateTime?> SidumiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}