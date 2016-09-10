using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrKaristusteValjavote : KarrValjavote, IXRoadXmlSerializable
    {
        public Option<IList<KarrOtsus>> ArhiveeritudOtsused { get; set; }
        public Option<IList<KarrOtsus>> KehtivadOtsused { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}