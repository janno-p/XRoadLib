using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrJareleparimisteValjavote : KarrValjavote
    {
        public class JareleparimisedType : IXRoadXmlSerializable
        {
            public Option<KarrJareleparimine> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<JareleparimisedType> Jareleparimised { get; set; }
        public Option<DateTime> VahemikuAlgusKP { get; set; }
        public Option<DateTime> VahemikuLoppKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}