using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetluseTeavitusAET : StandardTeavitus
    {
        public Option<Menetlus> Menetlus { get; set; }
        public Option<IList<Isik>> MenetlustNagevadIsikud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}