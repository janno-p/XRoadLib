using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class TegevuseKoht : Aadress
    {
        public Option<string> Selgitus { get; set; }
        public Option<DateTime?> TegevuseAlgusKP { get; set; }
        public Option<DateTime?> TegevuseLoppKP { get; set; }
        public Option<string> TegevuskohaNimetus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}