using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KohustiseVaatamineRequest : IXRoadXmlSerializable
    {
        public long ObjektID { get; set; }
        public Option<DateTime?> KehtivuseAlgusKP { get; set; }
        public Option<DateTime?> KehtivuseLoppKP { get; set; }
        public Option<long> VersiooniID { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}