using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KohtuasjaVaatamineRequest : IXRoadXmlSerializable
    {
        public Option<long> KohtuasjaObjektID { get; set; }
        public Option<long> KohtuasjaMenetluseObjektID { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}