using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Marksona : IXRoadXmlSerializable
    {
        public class AlamMarksonadType : IXRoadXmlSerializable
        {
            public Option<Marksona> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlamMarksonadType> AlamMarksonad { get; set; }
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }

        public class AsendatavadMarksonadType : IXRoadXmlSerializable
        {
            public Option<Marksona> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AsendatavadMarksonadType> AsendatavadMarksonad { get; set; }
        public Option<int> JrkNrHarus { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }
        public Option<string> Vaartus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}