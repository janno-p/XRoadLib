using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Maksekorraldus : IXRoadXmlSerializable
    {
        public Option<DateTime?> Aeg { get; set; }
        public Option<long?> MakseviisKL { get; set; }
        public Option<string> MaksjaNimi { get; set; }
        public Option<string> Nr { get; set; }
        public Option<long?> PankKL { get; set; }
        public Option<string> Selgitus { get; set; }
        public Option<decimal?> Summa { get; set; }
        public Option<long?> ValuutaKL { get; set; }
        public Option<string> Viitenumber { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}