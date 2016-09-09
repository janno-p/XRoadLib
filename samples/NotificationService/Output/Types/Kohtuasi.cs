using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kohtuasi : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<Asi> Asi { get; set; }
        public Option<string> KlientsysteemiID { get; set; }

        public class MenetlusedType : IXRoadXmlSerializable
        {
            public IList<Menetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusedType> Menetlused { get; set; }
        public Option<int?> MenetlusteArv { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> Pealkiri { get; set; }
        public Option<DateTime?> RegistreerimiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}