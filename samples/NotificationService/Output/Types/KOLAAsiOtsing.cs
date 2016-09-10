using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KOLAAsiOtsing : IXRoadXmlSerializable
    {
        public Option<string> AsjaKategooria { get; set; }
        public Option<string> AsjaLiik { get; set; }
        public Option<string> AsjaNumber { get; set; }
        public Option<bool?> KasAvalik { get; set; }
        public Option<string> Kohtunik { get; set; }
        public Option<string> Kohus { get; set; }
        public Option<DateTime?> LahendiKPAlates { get; set; }
        public Option<DateTime?> LahendiKPKuni { get; set; }
        public Option<string> Marksonad { get; set; }
        public Option<string> MenetlusaluneOsaline { get; set; }
        public Option<string> MuuOsaline { get; set; }
        public Option<string> Osaline { get; set; }
        public Option<KOLASeaduseSate> SeaduseSate { get; set; }
        public Option<int?> TulemusedAlates { get; set; }
        public Option<int?> TulemusedKuni { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}