using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrJareleparimisteValjavote : KarrValjavote, IXRoadXmlSerializable
    {
        public Option<IList<KarrJareleparimine>> Jareleparimised { get; set; }
        public Option<DateTime?> VahemikuAlgusKP { get; set; }
        public Option<DateTime?> VahemikuLoppKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}