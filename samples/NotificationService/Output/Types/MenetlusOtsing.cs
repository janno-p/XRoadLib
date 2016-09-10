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
        public Option<IList<long>> AsjaLiikKL { get; set; }
        public Option<long?> AsjaMenetlevRiikKL { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<long?> AsjaObjektID { get; set; }
        public Option<long?> AsjaSeisundKL { get; set; }
        public Option<DateTime?> AsjaSeisundKP { get; set; }
        public Option<DateTime?> AsjaSeisundKPVahemikuLoppKP { get; set; }
        public Option<IList<long>> AsjaStaadiumKL { get; set; }
        public Option<DateTime?> AsjaStaadiumKP { get; set; }
        public Option<bool> EXCLUDE { get; set; }
        public Option<long?> JrkNr { get; set; }
        public Option<IList<KohustisOtsing>> KehtivadKohustised { get; set; }
        public Option<DateTime?> KohtuasjaAlgusKP { get; set; }
        public Option<DateTime?> KohtuasjaAlgusKPVahemikuLoppKP { get; set; }
        public Option<string> KohtuasjaPealkiri { get; set; }
        public Option<string> KOLANR { get; set; }
        public Option<string> KvalifikatsioonidCSV { get; set; }
        public Option<IList<long>> LubatudObjektIDd { get; set; }
        public Option<string> MenetlejadCSV { get; set; }
        public Option<bool?> MenetlejaOnMaaramata { get; set; }
        public Option<string> MenetlenudValisriigiAsutus { get; set; }
        public Option<IList<JuriidilineIsikOtsing>> MenetlevadAsutused { get; set; }
        public Option<string> MenetlevadAsutusedCSV { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<string> MenetluseNrKoosJrkNr { get; set; }
        public Option<IList<MenetluseSisulineLiigitusOtsing>> MenetluseSisulineLiigitus { get; set; }
        public Option<IList<long>> MenetlusLiigiAlaLiikKL { get; set; }
        public Option<IList<long>> MenetlusLiikKL { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<IList<OsalineOtsing>> Osalised { get; set; }
        public Option<bool?> PuudutabAlaealisi { get; set; }
        public Option<DateTime?> RegistreerimiseKP { get; set; }
        public Option<DateTime?> RegistreerimiseKPVahemikuLoppKP { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }
        public Option<IList<long>> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<DateTime?> SeisundKPVahemikuLoppKP { get; set; }
        public Option<IList<SyyteosyndmusOtsing>> Syyteosyndmused { get; set; }
        public Option<IList<ToimingOtsing>> Toimingud { get; set; }
        public Option<IList<long>> TyypKL { get; set; }
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