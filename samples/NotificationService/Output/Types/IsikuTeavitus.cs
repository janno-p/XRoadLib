using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class IsikuTeavitus : StandardTeavitus, IXRoadXmlSerializable
    {
        public Option<Isik> Isik { get; set; }
        public Option<long> IsikuObjektID { get; set; }
        public Option<long> IsikuVersID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}