using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Asi : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<string> AsjaAlustamiseFaabula { get; set; }
        public Option<string> AsjaAlustamiseKvalifikatsioonCSV { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<boolean> AvaldamiseleMittekuuluv { get; set; }
        public Option<string> KlientsysteemiID { get; set; }

        public class KohtuasjadType : IXRoadXmlSerializable
        {
            public IList<Kohtuasi> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KohtuasjadType> Kohtuasjad { get; set; }
        public Option<long?> LiikKL { get; set; }

        public class MenetlusedType : IXRoadXmlSerializable
        {
            public IList<Menetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusedType> Menetlused { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<long?> ParitoluKL { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<long?> StaadiumKL { get; set; }
        public Option<DateTime?> StaadiumKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}