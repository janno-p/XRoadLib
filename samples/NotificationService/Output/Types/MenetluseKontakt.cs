using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetluseKontakt : IXRoadXmlSerializable
    {
        public Option<string> Email { get; set; }
        public Option<string> Lisainfo { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> Telefon { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}