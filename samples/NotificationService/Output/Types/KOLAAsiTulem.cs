using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KOLAAsiTulem : IXRoadXmlSerializable
    {
        public Option<IList<KOLAAsi>> Asi { get; set; }
        public Option<int> Kogus { get; set; }
        public Option<IList<KOLAXml>> Xml { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}