using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KvalifikatsiooniParagrahv : SeaduseSate
    {
        public Option<string> KlientsysteemiID { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<Syyteosyndmus> Syyteosyndmus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}