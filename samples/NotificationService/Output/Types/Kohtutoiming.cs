using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kohtutoiming : Toiming
    {
        public class AnnotatsioonidType : IXRoadXmlSerializable
        {
            public Option<Annotatsioon> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AnnotatsioonidType> Annotatsioonid { get; set; }
        public Option<int> IstungiNumber { get; set; }

        public class IstungisaalidType : IXRoadXmlSerializable
        {
            public Option<Istungisaal> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<IstungisaalidType> Istungisaalid { get; set; }

        public class KinniseksKuulutamiseAlusKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KinniseksKuulutamiseAlusKLType> KinniseksKuulutamiseAlusKL { get; set; }
        public Option<string> KinniseksKuulutamisePohjus { get; set; }
        public Option<long> KolleegiumiKoosseisKL { get; set; }
        public Option<long> KorraldavMaarusKL { get; set; }

        public class MojutatavadToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MojutatavadToimingudType> MojutatavadToimingud { get; set; }

        public class OigusaktidType : IXRoadXmlSerializable
        {
            public Option<Oigusakt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OigusaktidType> Oigusaktid { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}