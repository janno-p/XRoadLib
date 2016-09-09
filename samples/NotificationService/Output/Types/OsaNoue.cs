using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class OsaNoue : Kohustis
    {
        public Option<string> Maksmiseinfo { get; set; }
        public Option<string> MoistetudMurdosa { get; set; }
        public Option<NoudeOsa> Noudeosa { get; set; }
        public Option<boolean> OnRiigiOsaNoue { get; set; }
        public Option<string> TaodeldudMurdosa { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}