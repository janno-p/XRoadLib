using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KARRParing : IXRoadXmlSerializable
    {
        public Option<DateTime> Aeg { get; set; }
        public Option<long> EesmarkKL { get; set; }
        public Option<string> Infosysteem { get; set; }
        public Option<Isik> Isik { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<string> Pohjendus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}