using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguAllkirjastaminePealdisenaRequest : IXRoadXmlSerializable
    {
        public Toiming Toiming { get; set; }
        public ToiminguOsaline LoaAndja { get; set; }
        public Option<long?> MenetluseObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}