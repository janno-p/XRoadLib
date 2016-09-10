using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Sanktsioon : Kohustis
    {
        public Option<long?> AlaLiikKL { get; set; }
        public Option<bool?> AllutadaKaitumiskontrollile { get; set; }
        public Option<IList<SeaduseSate>> AluseksOlevadSeaduseSatted { get; set; }
        public Option<IList<SyydistusPunkt>> AluseksOlevadSyydistusPunktid { get; set; }
        public Option<string> AluseksOlevadSyydistusPunktidCSV { get; set; }
        public Option<IList<Syyteosyndmus>> AluseksOlevadSyyteosyndmused { get; set; }
        public Option<DateTime?> ArhiveerimiseKP { get; set; }
        public Option<int?> EelvangistusesViibitudPaevadeArv { get; set; }
        public Option<DateTime?> EnnetahtaegseVabanemiseAlgusKP { get; set; }
        public Option<DateTime?> KatseajaAlgusKP { get; set; }
        public Option<DateTime?> KatseajaTegelikLoppKP { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<int?> MoistetudAastaid { get; set; }
        public Option<int?> MoistetudKatseajaAastaid { get; set; }
        public Option<int?> MoistetudKatseajaKuid { get; set; }
        public Option<int?> MoistetudKatseajaPaevi { get; set; }
        public Option<int?> MoistetudKuid { get; set; }
        public Option<int?> MoistetudPaevamaaradeArv { get; set; }
        public Option<int?> MoistetudPaevi { get; set; }
        public Option<int?> MoistetudTunde { get; set; }
        public Option<bool?> OnArhiveeritud { get; set; }
        public Option<Sanktsioon> PohiKaristus { get; set; }
        public Option<int?> TaodeldavaidAastaid { get; set; }
        public Option<int?> TaodeldavaidKuid { get; set; }
        public Option<int?> TaodeldavaidPaevi { get; set; }
        public Option<int?> TaodeldavaidTunde { get; set; }
        public Option<int?> TaodeldavPaevamaaradeArv { get; set; }
        public Option<int?> TaodeldudKatseajaAastaid { get; set; }
        public Option<int?> TaodeldudKatseajaKuid { get; set; }
        public Option<int?> TaodeldudKatseajaPaevi { get; set; }
        public Option<DateTime?> TegelikKandmiseAlgusKP { get; set; }
        public Option<long?> TekeKL { get; set; }
        public Option<IList<Toiming>> Toimingud { get; set; }
        public Option<long?> TyypKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}