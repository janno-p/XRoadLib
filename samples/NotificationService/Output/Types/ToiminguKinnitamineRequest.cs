using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguKinnitamineRequest : IXRoadXmlSerializable
    {
        public Toiming Toiming { get; set; }
        public Menetlus Menetlus { get; set; }
        public ToiminguOsaline Menetleja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}