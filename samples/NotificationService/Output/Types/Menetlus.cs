using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Menetlus : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<DateTime?> ArhiveerimiseKP { get; set; }
        public Option<Asi> Asi { get; set; }
        public Option<DateTime?> AsjaAlgusKP { get; set; }
        public Option<string> AsjaAlustamiseFaabula { get; set; }
        public Option<string> AsjaAlustamiseKvalifikatsioonCSV { get; set; }
        public Option<long?> AsjaLiikKL { get; set; }
        public Option<long?> AsjaMenetlevRiikKL { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<long> AsjaObjektID { get; set; }
        public Option<long?> AsjaParitoluKL { get; set; }
        public Option<long?> AsjaSeisundKL { get; set; }
        public Option<DateTime?> AsjaSeisundKP { get; set; }
        public Option<long?> AsjaStaadiumKL { get; set; }
        public Option<DateTime?> AsjaStaadiumKP { get; set; }
        public Option<boolean> AvaldamiseleMittekuuluvAsi { get; set; }
        public Option<string> AvalikNimetus { get; set; }

        public class FailidType : IXRoadXmlSerializable
        {
            public IList<Fail> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<FailidType> Failid { get; set; }
        public Option<decimal?> HagiHind { get; set; }
        public Option<long?> HagiHindValuutaKL { get; set; }
        public Option<boolean> HagiTagamineEsialgneOiguskaitse { get; set; }

        public class HoiatusedType : IXRoadXmlSerializable
        {
            public IList<ETHoiatus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<HoiatusedType> Hoiatused { get; set; }
        public Option<int?> JrkNr { get; set; }
        public Option<boolean> KaebusOnEsitatud { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<Kohtuasi> Kohtuasi { get; set; }
        public Option<string> KOLANr { get; set; }
        public Option<MenetluseKontakt> Kontakt { get; set; }

        public class KuludType : IXRoadXmlSerializable
        {
            public IList<Kulu> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KuludType> Kulud { get; set; }
        public Option<DateTime?> LahendiTeatavakstegemiseAegKP { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<boolean> MenetlejaOnMaaramata { get; set; }
        public Option<DateTime?> MenetlemiseAlgusKP { get; set; }
        public Option<string> MenetlenudValisriigiAsutus { get; set; }

        public class MenetlevadAsutusedType : IXRoadXmlSerializable
        {
            public IList<JuriidilineIsik> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlevadAsutusedType> MenetlevadAsutused { get; set; }
        public Option<string> MenetlevadAsutusedCSV { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<string> MenetluseNrKoosJrkNr { get; set; }

        public class MenetluseSisulineLiigitusType : IXRoadXmlSerializable
        {
            public IList<MenetluseSisulineLiigitus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetluseSisulineLiigitusType> MenetluseSisulineLiigitus { get; set; }
        public Option<long?> MenetlusLiigiAlaLiikKL { get; set; }
        public Option<long?> MenetlusLiikKL { get; set; }

        public class MenetlusPostType : IXRoadXmlSerializable
        {
            public IList<MenetlusPost> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusPostType> MenetlusPost { get; set; }

        public class MuudOsalisedType : IXRoadXmlSerializable
        {
            public IList<Osaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MuudOsalisedType> MuudOsalised { get; set; }
        public Option<boolean> MuuJuurdepaasupiirang { get; set; }
        public Option<string> MuuJuurdepaasupiiranguPohjendus { get; set; }
        public Option<string> Nimetus { get; set; }

        public class NoudedType : IXRoadXmlSerializable
        {
            public IList<Noue> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<NoudedType> Nouded { get; set; }

        public class ObjektidType : IXRoadXmlSerializable
        {
            public IList<Objekt> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ObjektidType> Objektid { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<boolean> OigusabiMenetlusabiSaamine { get; set; }

        public class OsalisedType : IXRoadXmlSerializable
        {
            public IList<Osaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsalisedType> Osalised { get; set; }
        public Option<boolean> PuudutabAlaealisi { get; set; }
        public Option<DateTime?> RegistreerimiseKP { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }

        public class SanktsioonidType : IXRoadXmlSerializable
        {
            public IList<Sanktsioon> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SanktsioonidType> Sanktsioonid { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }

        public class SeotudAsjadType : IXRoadXmlSerializable
        {
            public IList<SeotudMenetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeotudAsjadType> SeotudAsjad { get; set; }

        public class SeotudMenetlusedType : IXRoadXmlSerializable
        {
            public IList<SeotudMenetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeotudMenetlusedType> SeotudMenetlused { get; set; }

        public class SyyteosyndmusedType : IXRoadXmlSerializable
        {
            public IList<Syyteosyndmus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SyyteosyndmusedType> Syyteosyndmused { get; set; }

        public class ToimingudType : IXRoadXmlSerializable
        {
            public IList<Toiming> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingudType> Toimingud { get; set; }
        public Option<long?> TyypKL { get; set; }
        public Option<string> VastutavadMenetlejadCSV { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}