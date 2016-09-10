using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class EsindusseoseLopetamineAETRequest : IXRoadXmlSerializable
    {
        public IList<long> OsaliseIsikuObjektID { get; set; }
        public long EsindajaIsikuObjektID { get; set; }
        public long AsjaObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}