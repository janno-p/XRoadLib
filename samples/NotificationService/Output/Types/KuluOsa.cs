using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KuluOsa : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<boolean> OnRiigiOsa { get; set; }
        public Option<Osaline> Osaline { get; set; }
        public Option<decimal?> OsaSumma { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<long?> ValuutaKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}