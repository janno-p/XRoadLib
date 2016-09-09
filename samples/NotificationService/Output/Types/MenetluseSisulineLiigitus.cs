using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetluseSisulineLiigitus : IXRoadXmlSerializable
    {
        public class AlamLiigitusedType : IXRoadXmlSerializable
        {
            public IList<MenetluseSisulineLiigitus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlamLiigitusedType> AlamLiigitused { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<long?> KaebuseProtestiLiikKL { get; set; }
        public Option<long?> KategooriaKL { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<string> LahenduseKirjeldus { get; set; }
        public Option<long?> LahenduseLisainfoKL { get; set; }
        public Option<long?> LahenduseMaaranudToiming { get; set; }
        public Option<long?> LahendusKL { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<long?> MenetluseTaiendavaLiigiAlaLiikKL { get; set; }
        public Option<long?> MenetluseTaiendavLiikKL { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<boolean> PeamineMaaramiseAlus { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<string> Sisu { get; set; }
        public Option<long?> SisulineLiigitusKL { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}