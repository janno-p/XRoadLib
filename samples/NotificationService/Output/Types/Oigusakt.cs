using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Oigusakt : IXRoadXmlSerializable
    {
        public Option<string> Alampunkt { get; set; }
        public Option<long> GlobaalID { get; set; }
        public Option<string> Loige { get; set; }
        public Option<string> Paragrahv { get; set; }
        public Option<string> Sisu { get; set; }
        public Option<long> TerviktekstiGrupiID { get; set; }
        public Option<string> Viide { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}