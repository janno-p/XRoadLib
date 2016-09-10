using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public abstract class IsikOtsing : IXRoadXmlSerializable
    {
        public Option<string> EelmisedKoosnimedCSV { get; set; }
        public Option<bool> EXCLUDE { get; set; }
        public Option<string> Kood { get; set; }
        public Option<string> Koosnimi { get; set; }
        public Option<IList<long>> MenetlusSubjektiLiikKL { get; set; }
        public Option<string> MuudeRiikideKoodid { get; set; }
        public Option<string> Nimi { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<IList<long>> OsalineKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}