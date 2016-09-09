using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Tagasimakse : IXRoadXmlSerializable
    {
        public Option<Kohustis> Kohustis { get; set; }
        public Option<string> Lisainfo { get; set; }
        public Option<string> MKR_ID { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<decimal?> RiigituludesseArvatavSumma { get; set; }
        public Option<long?> RiigituludesseArvatavSummaValuutaKL { get; set; }
        public Option<decimal?> TagastatavSumma { get; set; }
        public Option<long?> TagastatavSummaValuutaKL { get; set; }
        public Option<MakseRekvisiidid> TagastuseSaajaRekvisiidid { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}