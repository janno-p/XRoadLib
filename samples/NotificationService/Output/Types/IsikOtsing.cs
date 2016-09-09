using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class IsikOtsing : IXRoadXmlSerializable
    {
        public Option<string> EelmisedKoosnimedCSV { get; set; }
        public Option<boolean> EXCLUDE { get; set; }
        public Option<string> Kood { get; set; }
        public Option<string> Koosnimi { get; set; }

        public class MenetlusSubjektiLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusSubjektiLiikKLType> MenetlusSubjektiLiikKL { get; set; }
        public Option<string> MuudeRiikideKoodid { get; set; }
        public Option<string> Nimi { get; set; }
        public Option<long> ObjektID { get; set; }

        public class OsalineKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsalineKLType> OsalineKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}