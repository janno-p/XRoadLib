using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguTeavitusAET : IXRoadXmlSerializable
    {
        public Option<IList<Isik>> PohiFailiNagevadIsikud { get; set; }
        public Option<Toiming> Toiming { get; set; }
        public Option<IList<Isik>> ToimingutNagevadIsikud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}