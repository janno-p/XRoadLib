using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KARRParinguVastus : IXRoadXmlSerializable
    {
        public class KarrIsikuteValjavoteType : IXRoadXmlSerializable
        {
            public Option<KarrIsikuValjavote> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KarrIsikuteValjavoteType> KarrIsikuteValjavote { get; set; }
        public Option<DateTime> ParinguTegemiseAeg { get; set; }
        public Option<DateTime> ParinguTegemiseValitudAeg { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}