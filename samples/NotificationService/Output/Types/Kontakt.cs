using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kontakt : IXRoadXmlSerializable
    {
        public Option<Aadress> Aadress { get; set; }
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> KontaktTekst { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<string> Markused { get; set; }
        public Option<long> StaatusKL { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}