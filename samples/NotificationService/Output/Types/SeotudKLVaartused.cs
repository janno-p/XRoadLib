using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SeotudKLVaartused : IXRoadXmlSerializable
    {
        public Option<long> KLVaartusObjektID { get; set; }
        public Option<long> SeotudKLVaartusObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}