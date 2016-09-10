using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetluseKontaktideSeoseHaldamineRequest : IXRoadXmlSerializable
    {
        public IList<Menetlus> Menetlused { get; set; }
        public Option<MenetluseKontakt> MenetluseKontakt { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}