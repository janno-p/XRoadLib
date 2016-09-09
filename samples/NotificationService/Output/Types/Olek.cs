using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Olek : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<long> OlekKL { get; set; }

        public class OlekuMargeKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OlekuMargeKLType> OlekuMargeKL { get; set; }
        public Option<string> Selgitus { get; set; }
        public Option<string> ToiminguNR { get; set; }
        public Option<long> ToiminguObjektID { get; set; }
        public Option<string> ToiminguTegijaAsutusCSV { get; set; }
        public Option<string> ToiminguTegijaCSV { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}