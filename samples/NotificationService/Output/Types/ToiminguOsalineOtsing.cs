using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguOsalineOtsing : OsalineOtsing
    {
        public Option<DateTime?> AllkirjastamiseKP { get; set; }
        public Option<long?> AmetKL { get; set; }
        public Option<string> KattesaajaCSV { get; set; }
        public Option<DateTime?> KattesaamiseKP { get; set; }

        public class MenetluseMenetlevadAsutusedType : IXRoadXmlSerializable
        {
            public IList<JuriidilineIsikOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetluseMenetlevadAsutusedType> MenetluseMenetlevadAsutused { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<long> MenetluseObjektID { get; set; }
        public Option<DateTime?> ToiminguAlgusKP { get; set; }
        public Option<DateTime?> ToiminguAlgusKPVahemikuLoppKP { get; set; }
        public Option<long> ToiminguLiikKL { get; set; }

        public class ToiminguMenetlejadType : IXRoadXmlSerializable
        {
            public IList<ToiminguOsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToiminguMenetlejadType> ToiminguMenetlejad { get; set; }
        public Option<string> ToiminguNR { get; set; }
        public Option<long> ToiminguObjektID { get; set; }
        public Option<long> ToiminguosaliseLiikKL { get; set; }

        public class ToimingutMenetlevAsutusType : IXRoadXmlSerializable
        {
            public IList<JuriidilineIsikOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingutMenetlevAsutusType> ToimingutMenetlevAsutus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}