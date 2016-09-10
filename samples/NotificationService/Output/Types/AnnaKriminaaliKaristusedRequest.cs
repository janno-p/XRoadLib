using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class AnnaKriminaaliKaristusedRequest : IXRoadXmlSerializable
    {
        public long IsikuObjektID { get; set; }
        public Option<IList<long>> SeisundKLFilter { get; set; }
        public Option<IList<long>> TyypKLFilter { get; set; }
        public Option<IList<long>> LiikKLFilter { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}