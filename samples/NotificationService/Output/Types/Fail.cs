using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Fail : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<string> Failitee { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<string> Laiend { get; set; }
        public Option<long> Maht { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<Stream> Sisu { get; set; }
        public Option<DateTime?> ViimatiMuudetud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}