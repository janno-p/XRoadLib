using Optional;
using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Aadress : IXRoadXmlSerializable
    {
        public Option<string> Aadresskood { get; set; }
        public Option<string> AadressTekst { get; set; }
        public Option<int?> ADR_ID { get; set; }
        public Option<long?> AlevKylaLinnaosaKL { get; set; }
        public Option<string> Hooneosa { get; set; }
        public Option<string> Kirjeldus { get; set; }
        public Option<long?> LinnValdKL { get; set; }
        public Option<long?> MaakondKL { get; set; }
        public Option<string> MajaKrunt { get; set; }
        public Option<decimal?> PunktiXKoordinaat { get; set; }
        public Option<decimal?> PunktiYKoordinaat { get; set; }
        public Option<long?> RiikKL { get; set; }
        public Option<string> Sihtnumber { get; set; }
        public Option<string> Tanav { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}