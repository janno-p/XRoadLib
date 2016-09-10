using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class JuriidilineIsikOtsing : IsikOtsing
    {
        public Option<bool?> KaristatudIsik { get; set; }
        public Option<IList<KontaktOtsing>> Kontaktid { get; set; }
        public Option<bool?> OnKehtiv { get; set; }
        public Option<bool?> OnSysteemne { get; set; }
        public Option<bool?> OtsiIlmaAlamasutusteta { get; set; }
        public Option<DateTime?> TegevusAlgusKP { get; set; }
        public Option<DateTime?> TegevusLoppKP { get; set; }
        public Option<IList<long>> VormKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}