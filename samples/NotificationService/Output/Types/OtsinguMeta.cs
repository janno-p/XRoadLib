using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class OtsinguMeta : IXRoadXmlSerializable
    {
        public Option<long?> OtsinguTulemiKujuKL { get; set; }
        public Option<int> PageNum { get; set; }
        public Option<int> PageSize { get; set; }
        public Option<IList<SorditavVali>> SorditavadValjad { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}