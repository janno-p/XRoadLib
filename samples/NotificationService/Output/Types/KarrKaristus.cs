using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrKaristus : IXRoadXmlSerializable
    {
        public Option<bool?> AllutadaKaitumiskontrollile { get; set; }
        public Option<int?> EelvangistusesViibitudPaevadeArv { get; set; }
        public Option<DateTime?> KatseajaAlgusKP { get; set; }
        public Option<DateTime?> KatseajaLoppKP { get; set; }
        public Option<IList<string>> Kvalifikatsioon { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<IList<KarrKaristus>> LisaKaristused { get; set; }
        public Option<int?> MoistetudAastaid { get; set; }
        public Option<int?> MoistetudKatseajaAastaid { get; set; }
        public Option<int?> MoistetudKatseajaKuid { get; set; }
        public Option<int?> MoistetudKatseajaPaevi { get; set; }
        public Option<int?> MoistetudKuid { get; set; }
        public Option<int?> MoistetudMaaradeArv { get; set; }
        public Option<int?> MoistetudPaevi { get; set; }
        public Option<decimal?> MoistetudSumma { get; set; }
        public Option<long?> MoistetudSummaValuutaKL { get; set; }
        public Option<int?> MoistetudTunde { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<long?> RakendamiseLoppAlusKL { get; set; }
        public Option<DateTime?> RakendamiseTahtaegKP { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> TaitmiseKP { get; set; }
        public Option<DateTime?> TingimisiVabastamiseKP { get; set; }
        public Option<DateTime?> ToimepanemiseKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}