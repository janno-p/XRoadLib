using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class FailiTeavitus : StandardTeavitus
    {
        public Option<IList<Fail>> Failid { get; set; }
        public Option<Fail> PohiFail { get; set; }
        public Option<long> ToiminguObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}