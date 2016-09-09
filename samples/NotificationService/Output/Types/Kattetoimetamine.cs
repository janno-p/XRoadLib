using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kattetoimetamine : IXRoadXmlSerializable
    {
        public Option<DateTime?> KattesaamiseKP { get; set; }
        public Option<long?> KattesaamiseViisKL { get; set; }

        public class KattesaanudIsikudType : IXRoadXmlSerializable
        {
            public IList<FyysilineIsik> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KattesaanudIsikudType> KattesaanudIsikud { get; set; }
        public Option<string> Kommentaar { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> SaadetiseID { get; set; }
        public Option<DateTime?> SaatmiseKP { get; set; }
        public Option<long?> SaatmiseViisKL { get; set; }
        public Option<DateTime?> TagastamiseKP { get; set; }
        public Option<long?> TagastamisePohjusKL { get; set; }
        public Option<string> TyhistamisePohjendus { get; set; }
        public Option<boolean> Tyhistatud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}