using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguEdastamiseVaatamineRequest : IXRoadXmlSerializable
    {
        public Option<long?> MenetluseObjektID { get; set; }
        public Option<IList<long>> ToiminguteObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}