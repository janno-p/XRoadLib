using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguTeavitus : StandardTeavitus
    {
        public Option<string> AsjaNR { get; set; }
        public Option<long?> AsjaObjektID { get; set; }
        public Option<Toiming> Toiming { get; set; }
        public Option<long?> ToiminguAlaLiikKL { get; set; }
        public Option<string> ToiminguAlaLiikKLVaartus { get; set; }
        public Option<long?> ToiminguLiikKL { get; set; }
        public Option<string> ToiminguLiikKLVaartus { get; set; }
        public Option<long?> ToiminguMenetluseMenetlevAsutusObjektID { get; set; }
        public Option<string> ToiminguNR { get; set; }
        public Option<long> ToiminguObjektID { get; set; }
        public Option<string> ToiminguOsalisedCSV { get; set; }
        public Option<string> ToiminguTegijaAsutusCSV { get; set; }
        public Option<long?> ToiminguTegijaAsutusObjektID { get; set; }
        public Option<string> ToiminguTegijaCSV { get; set; }
        public Option<long> ToiminguVersID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}