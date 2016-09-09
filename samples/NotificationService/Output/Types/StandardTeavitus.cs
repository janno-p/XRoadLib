using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class StandardTeavitus : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> AsutusCSV { get; set; }
        public Option<long?> AsutusObjektID { get; set; }
        public Option<string> IsikCSV { get; set; }
        public Option<long?> IsikObjektID { get; set; }

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
        public Option<long?> TeavitamiseEesmarkKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}