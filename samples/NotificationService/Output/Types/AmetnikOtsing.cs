using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class AmetnikOtsing : IsikOtsing
    {
        public class AmetKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AmetKLType> AmetKL { get; set; }
        public Option<string> Eesnimi { get; set; }
        public Option<JuriidilineIsikOtsing> Organisatsiooniyksus { get; set; }
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