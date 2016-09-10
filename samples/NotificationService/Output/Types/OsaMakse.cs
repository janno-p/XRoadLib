using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class OsaMakse : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<Kohustis> Kohustis { get; set; }
        public Option<int?> MoistetudPaevamaaradeArv { get; set; }
        public Option<decimal?> MoistetudSumma { get; set; }
        public Option<long?> MoistetudSummaValuutaKL { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<DateTime?> RakendumiseLoppKP { get; set; }
        public Option<DateTime?> RakendumiseTahtaegKP { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<decimal?> SissenoutavSumma { get; set; }
        public Option<long?> SissenoutavSummaValuutaKL { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<long?> SundtaitmiseSeisundKL { get; set; }
        public Option<bool?> Taidetud { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}