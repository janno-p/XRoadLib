using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Objekt : IXRoadXmlSerializable
    {
        public Option<Aadress> Aadress { get; set; }

        public class AlaLiigidKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlaLiigidKLType> AlaLiigidKL { get; set; }

        public class AlaStaatusedKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlaStaatusedKLType> AlaStaatusedKL { get; set; }
        public Option<DateTime> AlgusKP { get; set; }

        public class FailidType : IXRoadXmlSerializable
        {
            public Option<Fail> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<FailidType> Failid { get; set; }
        public Option<string> Kirjeldus { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<decimal> Kogus { get; set; }

        public class LiigidKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LiigidKLType> LiigidKL { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime> MuutmiseKP { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<string> Number { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<long> PakendiLiikKL { get; set; }
        public Option<string> PakendiNR { get; set; }
        public Option<decimal> RahaVaartus { get; set; }
        public Option<long> SeisundKL { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime> SisestamiseKP { get; set; }

        public class StaatusedKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<StaatusedKLType> StaatusedKL { get; set; }
        public Option<string> Sulgeja { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }

        public class ToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingudType> Toimingud { get; set; }
        public Option<long> TyypKL { get; set; }
        public Option<long> ValuutaKL { get; set; }
        public Option<long> YhikKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}