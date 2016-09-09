using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrValjavote : IXRoadXmlSerializable
    {
        public Option<boolean> Arhiivist { get; set; }
        public Option<long?> AsjaLiikKL { get; set; }
        public Option<Isik> Isik { get; set; }
        public Option<KarrJareleparimine> Jareleparimine { get; set; }
        public Option<boolean> PiiratudParing { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}