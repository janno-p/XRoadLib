using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class IstungisaalOtsing : IXRoadXmlSerializable
    {
        public Option<JuriidilineIsikOtsing> Kohtumaja { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<long?> ObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}