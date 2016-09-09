using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguTeavitusAET : IXRoadXmlSerializable
    {
        public class PohiFailiNagevadIsikudType : IXRoadXmlSerializable
        {
            public IList<Isik> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<PohiFailiNagevadIsikudType> PohiFailiNagevadIsikud { get; set; }
        public Option<Toiming> Toiming { get; set; }

        public class ToimingutNagevadIsikudType : IXRoadXmlSerializable
        {
            public IList<Isik> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingutNagevadIsikudType> ToimingutNagevadIsikud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}