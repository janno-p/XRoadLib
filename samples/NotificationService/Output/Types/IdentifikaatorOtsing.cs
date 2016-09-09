using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class IdentifikaatorOtsing : IXRoadXmlSerializable
    {
        public Option<string> IdentifikaatoriTyyp { get; set; }
        public Option<string> KLientsysteemiID { get; set; }
        public Option<long> ObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}