using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class FyysilineIsik : Isik
    {
        public Option<string> DokumendiNimetus { get; set; }
        public Option<string> DokumendiNR { get; set; }
        public Option<long> DokumendiTyypKL { get; set; }
        public Option<string> Eesnimi { get; set; }
        public Option<long> EmakeelKL { get; set; }
        public Option<string> EmaNimi { get; set; }
        public Option<long> HaridusKL { get; set; }
        public Option<string> HyydNimed { get; set; }
        public Option<string> IsaNimi { get; set; }
        public Option<DateTime> KIRArvelolekAlgusKP { get; set; }
        public Option<long> KIRArvelolekKL { get; set; }
        public Option<DateTime> KIRArvelolekLoppKP { get; set; }
        public Option<long> KodakondsusKL { get; set; }

        public class MitmikKodakondsusKLType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MitmikKodakondsusKLType> MitmikKodakondsusKL { get; set; }
        public Option<long> PerekonnaseisKL { get; set; }
        public Option<long> SuguKL { get; set; }
        public Option<long> SuhtluskeelKL { get; set; }
        public Option<DateTime> SurmaKP { get; set; }
        public Option<string> Synnikoht { get; set; }
        public Option<DateTime> SynniKP { get; set; }
        public Option<string> TeisedNimed { get; set; }

        public class TookohtvOppeasutusType : IXRoadXmlSerializable
        {
            public Option<Ametikoht> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TookohtvOppeasutusType> TookohtvOppeasutus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}