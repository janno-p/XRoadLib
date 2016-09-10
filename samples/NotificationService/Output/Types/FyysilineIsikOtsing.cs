using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class FyysilineIsikOtsing : IsikOtsing
    {
        public Option<string> AmetikohaNimetus { get; set; }
        public Option<string> DokumendiNimetus { get; set; }
        public Option<string> DokumendiNR { get; set; }
        public Option<long> DokumendiTyypKL { get; set; }
        public Option<string> Eesnimi { get; set; }
        public Option<string> ElukohtCSV { get; set; }
        public Option<string> EmaNimi { get; set; }
        public Option<string> IsaNimi { get; set; }
        public Option<bool?> KaristatudIsik { get; set; }
        public Option<long> KodakondsusKL { get; set; }
        public Option<IList<long>> MitmikKodakondsusKL { get; set; }
        public Option<long> SuguKL { get; set; }
        public Option<DateTime?> SurmaKP { get; set; }
        public Option<DateTime?> SynniKP { get; set; }
        public Option<DateTime?> SynniKPVahemikuLoppKP { get; set; }
        public Option<string> TeisedNimed { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}