using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class JuriidilineIsik : Isik
    {
        public Option<IList<JuriidilineIsik>> OrganisatsiooniYksused { get; set; }
        public Option<DateTime?> TegevusAlgusKP { get; set; }
        public Option<DateTime?> TegevusLoppKP { get; set; }
        public Option<long?> VormKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}