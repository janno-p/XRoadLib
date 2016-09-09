using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SeaduseSate : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> JaguNR { get; set; }
        public Option<long?> JaoNimetusKL { get; set; }
        public Option<long?> JaotiseNimetusKL { get; set; }
        public Option<string> JaotisNR { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<string> Loige { get; set; }
        public Option<string> LoigePrimm { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> Paragrahv { get; set; }
        public Option<string> ParagrahvPrimm { get; set; }
        public Option<long?> PeatykiNimetusKL { get; set; }
        public Option<string> PeatykkNR { get; set; }
        public Option<string> Punkt { get; set; }
        public Option<string> PunktPrimm { get; set; }
        public Option<long?> SeadustikKL { get; set; }
        public Option<string> Tekst { get; set; }
        public Option<string> TekstLyhendatult { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}