using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class RiigiOigusabi : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<decimal> Kaugus { get; set; }
        public Option<long> KaugusYhikKL { get; set; }
        public Option<string> Kirjeldus { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<decimal> Koefitsient { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<decimal> MaaratudTasuSumma { get; set; }
        public Option<decimal> MaaratudTasuSummaKM { get; set; }
        public Option<long> MaaratudTasuSummaValuutaKL { get; set; }
        public Option<string> Markused { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<decimal> OsutamiseAeg { get; set; }
        public Option<long> OsutamiseAegYhikKL { get; set; }
        public Option<DateTime> OsutamiseAlgusKP { get; set; }
        public Option<string> OsutamiseKoht { get; set; }
        public Option<DateTime> OsutamiseLoppKP { get; set; }
        public Option<decimal> TasuSumma { get; set; }
        public Option<decimal> TasuSummaKM { get; set; }
        public Option<long> TasuSummaValuutaKL { get; set; }
        public Option<long> TyypKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}