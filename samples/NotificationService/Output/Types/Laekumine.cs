using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Laekumine : IXRoadXmlSerializable
    {
        public Option<string> KontoNR { get; set; }
        public Option<DateTime> LaekumiseKP { get; set; }
        public Option<decimal> LaekumiseSumma { get; set; }
        public Option<long> LaekumiseSummaValuutaKL { get; set; }
        public Option<string> MakseId { get; set; }
        public Option<string> MakseSelgitus { get; set; }
        public Option<long> MakseViisKL { get; set; }
        public Option<string> MaksjaKood { get; set; }
        public Option<string> MaksjaNimi { get; set; }
        public Option<DateTime> MaksmiseKP { get; set; }
        public Option<long> MuutuseLiikKL { get; set; }
        public Option<string> Viitenumber { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}