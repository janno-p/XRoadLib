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
        public Option<bool> EXCLUDE { get; set; }
        public Option<IList<long>> KaebuseProtestiLiikKL { get; set; }
        public Option<IList<long>> KategooriaKL { get; set; }
        public Option<string> LahenduseKirjeldus { get; set; }
        public Option<long?> LahenduseLisainfoKL { get; set; }
        public Option<long?> LahenduseMaaranudToimingObjektID { get; set; }
        public Option<long?> LahendusKL { get; set; }
        public Option<IList<long>> MenetluseTaiendavaLiigiAlaliikKL { get; set; }
        public Option<IList<long>> MenetluseTaiendavLiikKL { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<bool?> PeamineMaaramiseAlus { get; set; }
        public Option<string> Sisu { get; set; }
        public Option<IList<long>> SisulineLiigitusKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}