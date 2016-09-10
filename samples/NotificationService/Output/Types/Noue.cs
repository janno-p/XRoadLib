using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Noue : IXRoadXmlSerializable
    {
        public Option<long?> AlaLiikKL { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<IList<Kulu>> AluseksOlevadKulud { get; set; }
        public Option<IList<Syyteosyndmus>> AluseksOlevadSyyteosyndmused { get; set; }
        public Option<IList<Kulu>> AluselTekkinudKulud { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<bool?> KohustuslikNoue { get; set; }
        public Option<DateTime?> KoostamiseKP { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<string> NoudeEse { get; set; }
        public Option<IList<NoudeOsa>> Noudeosad { get; set; }
        public Option<decimal?> NoudeSumma { get; set; }
        public Option<long?> NoudeSummaValuutaKL { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<Osaline> OigustatudIsik { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<long?> TyypKL { get; set; }
        public Option<DateTime?> VerAlgusKP { get; set; }
        public Option<DateTime?> VerLoppKP { get; set; }
        public Option<long> VersID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}