using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kulu : IXRoadXmlSerializable
    {
        public Option<long?> AlaLiikKL { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<IList<Noue>> AluseksOlevadNouded { get; set; }
        public Option<IList<Noue>> AluselTekkinudNouded { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<IList<KuluOsa>> KandmiseOsad { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<Isik> KuluSaajaIsik { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<Menetlus> Menetlus { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<long?> PohjustamiseJagamiseViisKL { get; set; }
        public Option<IList<KuluOsa>> PohjustamiseOsad { get; set; }
        public Option<string> Selgitus { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<decimal?> Summa { get; set; }
        public Option<DateTime?> TekkimiseKP { get; set; }
        public Option<Toiming> Toiming { get; set; }
        public Option<long?> TyypKL { get; set; }
        public Option<long?> ValuutaKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}