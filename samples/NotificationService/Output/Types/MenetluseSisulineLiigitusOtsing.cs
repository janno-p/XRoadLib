using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetluseSisulineLiigitusOtsing : IXRoadXmlSerializable
    {
        public Option<boolean> EXCLUDE { get; set; }

        public class KaebuseProtestiLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KaebuseProtestiLiikKLType> KaebuseProtestiLiikKL { get; set; }

        public class KategooriaKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KategooriaKLType> KategooriaKL { get; set; }
        public Option<string> LahenduseKirjeldus { get; set; }
        public Option<long?> LahenduseLisainfoKL { get; set; }
        public Option<long?> LahenduseMaaranudToimingObjektID { get; set; }
        public Option<long?> LahendusKL { get; set; }

        public class MenetluseTaiendavaLiigiAlaliikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetluseTaiendavaLiigiAlaliikKLType> MenetluseTaiendavaLiigiAlaliikKL { get; set; }

        public class MenetluseTaiendavLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetluseTaiendavLiikKLType> MenetluseTaiendavLiikKL { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<boolean> PeamineMaaramiseAlus { get; set; }
        public Option<string> Sisu { get; set; }

        public class SisulineLiigitusKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SisulineLiigitusKLType> SisulineLiigitusKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}