using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class NoudeOsa : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<long> JagamiseViisKL { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<DateTime> LoppKP { get; set; }
        public Option<Noue> Noue { get; set; }
        public Option<long> ObjektID { get; set; }

        public class OsaNoudedType : IXRoadXmlSerializable
        {
            public Option<OsaNoue> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsaNoudedType> OsaNouded { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}