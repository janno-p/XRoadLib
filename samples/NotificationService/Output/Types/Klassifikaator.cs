using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Klassifikaator : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> Kirjeldus { get; set; }

        public class KLVaartusedType : IXRoadXmlSerializable
        {
            public Option<KLVaartus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KLVaartusedType> KLVaartused { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<string> Objekt { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> Tunnus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}