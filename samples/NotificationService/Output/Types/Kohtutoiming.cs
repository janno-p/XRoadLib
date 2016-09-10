using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kohtutoiming : Toiming, IXRoadXmlSerializable
    {
        public Option<IList<Annotatsioon>> Annotatsioonid { get; set; }
        public Option<int?> IstungiNumber { get; set; }
        public Option<IList<Istungisaal>> Istungisaalid { get; set; }
        public Option<IList<long>> KinniseksKuulutamiseAlusKL { get; set; }
        public Option<string> KinniseksKuulutamisePohjus { get; set; }
        public Option<long?> KolleegiumiKoosseisKL { get; set; }
        public Option<long?> KorraldavMaarusKL { get; set; }
        public Option<IList<Toiming>> MojutatavadToimingud { get; set; }
        public Option<IList<Oigusakt>> Oigusaktid { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}