using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KARRFaktiParinguVastus : IXRoadXmlSerializable
    {
        public Option<Isik> KaristatudIsik { get; set; }
        public Option<long> ParinguID { get; set; }
        public Option<DateTime> ParinguTegemiseAeg { get; set; }
        public Option<DateTime?> ParinguTegemiseValitudAeg { get; set; }
        public Option<long> PdfFailiObjektID { get; set; }
        public Option<IList<Toiming>> Toimingud { get; set; }
        public Option<bool?> Valideeritud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}