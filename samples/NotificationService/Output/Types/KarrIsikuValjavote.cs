using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrIsikuValjavote : IXRoadXmlSerializable
    {
        public Option<Isik> Isik { get; set; }
        public Option<long> ParinguID { get; set; }
        public Option<long> PdfFailiObjektID { get; set; }
        public Option<bool?> Valideeritud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}