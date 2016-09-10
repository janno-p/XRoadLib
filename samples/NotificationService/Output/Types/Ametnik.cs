using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Ametnik : Isik, IXRoadXmlSerializable
    {
        public Option<long?> AmetKL { get; set; }
        public Option<string> Eesnimi { get; set; }
        public Option<JuriidilineIsik> Organisatsiooniyksus { get; set; }
        public Option<string> OrganisatsiooniyksusCSV { get; set; }
        public Option<long> TootamiseObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}