using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class MenetlusOtsing : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<DateTime?> AlgusKPVahemikuLoppKP { get; set; }
        public Option<DateTime?> AsjaAlgusKP { get; set; }
        public Option<DateTime?> AsjaAlgusKPVahemikuLoppKP { get; set; }

        public class AsjaLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AsjaLiikKLType> AsjaLiikKL { get; set; }
        public Option<long?> AsjaMenetlevRiikKL { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<long?> AsjaObjektID { get; set; }
        public Option<long?> AsjaSeisundKL { get; set; }
        public Option<DateTime?> AsjaSeisundKP { get; set; }
        public Option<DateTime?> AsjaSeisundKPVahemikuLoppKP { get; set; }

        public class AsjaStaadiumKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AsjaStaadiumKLType> AsjaStaadiumKL { get; set; }
        public Option<DateTime?> AsjaStaadiumKP { get; set; }
        public Option<boolean> EXCLUDE { get; set; }
        public Option<long?> JrkNr { get; set; }

        public class KehtivadKohustisedType : IXRoadXmlSerializable
        {
            public IList<KohustisOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KehtivadKohustisedType> KehtivadKohustised { get; set; }
        public Option<DateTime?> KohtuasjaAlgusKP { get; set; }
        public Option<DateTime?> KohtuasjaAlgusKPVahemikuLoppKP { get; set; }
        public Option<string> KohtuasjaPealkiri { get; set; }
        public Option<string> KOLANR { get; set; }
        public Option<string> KvalifikatsioonidCSV { get; set; }

        public class LubatudObjektIDdType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LubatudObjektIDdType> LubatudObjektIDd { get; set; }
        public Option<string> MenetlejadCSV { get; set; }
        public Option<boolean> MenetlejaOnMaaramata { get; set; }
        public Option<string> MenetlenudValisriigiAsutus { get; set; }

        public class MenetlevadAsutusedType : IXRoadXmlSerializable
        {
            public IList<JuriidilineIsikOtsing> item { get; set; }

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
            public IList<MenetluseSisulineLiigitusOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetluseSisulineLiigitusType> MenetluseSisulineLiigitus { get; set; }

        public class MenetlusLiigiAlaLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusLiigiAlaLiikKLType> MenetlusLiigiAlaLiikKL { get; set; }

        public class MenetlusLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusLiikKLType> MenetlusLiikKL { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<long?> ObjektID { get; set; }

        public class OsalisedType : IXRoadXmlSerializable
        {
            public IList<OsalineOtsing> item { get; set; }

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
        public Option<DateTime?> RegistreerimiseKPVahemikuLoppKP { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }

        public class SeisundKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeisundKLType> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<DateTime?> SeisundKPVahemikuLoppKP { get; set; }

        public class SyyteosyndmusedType : IXRoadXmlSerializable
        {
            public IList<SyyteosyndmusOtsing> item { get; set; }

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
            public IList<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingudType> Toimingud { get; set; }

        public class TyypKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TyypKLType> TyypKL { get; set; }
        public Option<DateTime?> ViimaseToiminguAlgusKP { get; set; }
        public Option<DateTime?> ViimaseToiminguAlgusKPVahemikuLoppKP { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}