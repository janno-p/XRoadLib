using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ETHoiatus : IXRoadXmlSerializable
    {
        public Option<string> VeaGrupp { get; set; }
        public Option<string> VeaKood { get; set; }
        public Option<string> VeaSonum { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}