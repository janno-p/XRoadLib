using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Konto : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<string> KontoHaldur { get; set; }
        public Option<string> KontoNR { get; set; }
        public Option<Isik> KontoOmanik { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}