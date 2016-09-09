using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Sanktsioon : Kohustis
    {
        public Option<long> AlaLiikKL { get; set; }
        public Option<boolean> AllutadaKaitumiskontrollile { get; set; }

        public class AluseksOlevadSeaduseSattedType : IXRoadXmlSerializable
        {
            public Option<SeaduseSate> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AluseksOlevadSeaduseSattedType> AluseksOlevadSeaduseSatted { get; set; }

        public class AluseksOlevadSyydistusPunktidType : IXRoadXmlSerializable
        {
            public Option<SyydistusPunkt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AluseksOlevadSyydistusPunktidType> AluseksOlevadSyydistusPunktid { get; set; }
        public Option<string> AluseksOlevadSyydistusPunktidCSV { get; set; }

        public class AluseksOlevadSyyteosyndmusedType : IXRoadXmlSerializable
        {
            public Option<Syyteosyndmus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AluseksOlevadSyyteosyndmusedType> AluseksOlevadSyyteosyndmused { get; set; }
        public Option<DateTime> ArhiveerimiseKP { get; set; }
        public Option<int> EelvangistusesViibitudPaevadeArv { get; set; }
        public Option<DateTime> EnnetahtaegseVabanemiseAlgusKP { get; set; }
        public Option<DateTime> KatseajaAlgusKP { get; set; }
        public Option<DateTime> KatseajaTegelikLoppKP { get; set; }
        public Option<long> LiikKL { get; set; }
        public Option<int> MoistetudAastaid { get; set; }
        public Option<int> MoistetudKatseajaAastaid { get; set; }
        public Option<int> MoistetudKatseajaKuid { get; set; }
        public Option<int> MoistetudKatseajaPaevi { get; set; }
        public Option<int> MoistetudKuid { get; set; }
        public Option<int> MoistetudPaevamaaradeArv { get; set; }
        public Option<int> MoistetudPaevi { get; set; }
        public Option<int> MoistetudTunde { get; set; }
        public Option<boolean> OnArhiveeritud { get; set; }
        public Option<Sanktsioon> PohiKaristus { get; set; }
        public Option<int> TaodeldavaidAastaid { get; set; }
        public Option<int> TaodeldavaidKuid { get; set; }
        public Option<int> TaodeldavaidPaevi { get; set; }
        public Option<int> TaodeldavaidTunde { get; set; }
        public Option<int> TaodeldavPaevamaaradeArv { get; set; }
        public Option<int> TaodeldudKatseajaAastaid { get; set; }
        public Option<int> TaodeldudKatseajaKuid { get; set; }
        public Option<int> TaodeldudKatseajaPaevi { get; set; }
        public Option<DateTime> TegelikKandmiseAlgusKP { get; set; }
        public Option<long> TekeKL { get; set; }

        public class ToimingudType : IXRoadXmlSerializable
        {
            public Option<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingudType> Toimingud { get; set; }
        public Option<long> TyypKL { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}