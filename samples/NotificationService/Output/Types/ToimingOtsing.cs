using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToimingOtsing : IXRoadXmlSerializable
    {
        public Option<IList<long>> AlaLiigiTapsustusKL { get; set; }
        public Option<IList<long>> AlaLiikKL { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<DateTime?> AlgusKPVahemikuLoppKP { get; set; }
        public Option<long?> AllikaksOlevKlientSysteemKL { get; set; }
        public Option<IList<SeaduseSateOtsing>> Alus { get; set; }
        public Option<IList<ToimingOtsing>> AlusToimingud { get; set; }
        public Option<IList<AnnotatsioonOtsing>> Annotatsioonid { get; set; }
        public Option<IList<FailOtsing>> AvalikustatudFailid { get; set; }
        public Option<IList<ToiminguOsalineOtsing>> ByrooToiminguosalised { get; set; }
        public Option<DateTime?> DokumendiSaabumiseKP { get; set; }
        public Option<DateTime?> DokumendiSaabumiseKPVahemikuLoppKP { get; set; }
        public Option<string> EcliNR { get; set; }
        public Option<bool?> EdasiKaevatud { get; set; }
        public Option<long?> EdastamiseViisKL { get; set; }
        public Option<bool?> EriarvamusEsitatud { get; set; }
        public Option<long?> EsitamiseViisKL { get; set; }
        public Option<bool> EXCLUDE { get; set; }
        public Option<IList<FailOtsing>> Failid { get; set; }
        public Option<IList<IstungisaalOtsing>> Istungisaalid { get; set; }
        public Option<DateTime?> JoustumisKP { get; set; }
        public Option<long?> KohustisObjektID { get; set; }
        public Option<IList<ToiminguOsalineOtsing>> KoikToiminguosalised { get; set; }
        public Option<DateTime?> LabiviimiseAlgusKP { get; set; }
        public Option<DateTime?> LabiviimiseAlgusKPVahemikuLoppKP { get; set; }
        public Option<DateTime?> LabiviimiseLoppKP { get; set; }
        public Option<long?> LabiviimiseVormKL { get; set; }
        public Option<DateTime?> LahendiKuulutamiseAeg { get; set; }
        public Option<DateTime?> LahendiKuulutamiseAegVahemikuLoppKP { get; set; }
        public Option<IList<long>> LahenduseLiikKL { get; set; }
        public Option<IList<long>> LiikKL { get; set; }
        public Option<IList<ToimingOtsing>> LisaToimingud { get; set; }
        public Option<IList<long>> MarksonastatusKL { get; set; }
        public Option<string> Markused { get; set; }
        public Option<string> MeieAsjaajamisNR { get; set; }
        public Option<MenetlusOtsing> Menetlus { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<long?> MenetluseObjektID { get; set; }
        public Option<IList<MenetlusPostOtsing>> MenetlusPost { get; set; }
        public Option<IList<ToimingOtsing>> MojutatavadToimingud { get; set; }
        public Option<IList<ToimingOtsing>> MojutavadToimingud { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<bool?> OnAvalikustatud { get; set; }
        public Option<FailOtsing> PohiFail { get; set; }
        public Option<IList<ToimingOtsing>> PohiToimingud { get; set; }
        public Option<string> Pohjendus { get; set; }
        public Option<DateTime?> PostitamiseKP { get; set; }
        public Option<DateTime?> PostitamiseKPVahemikuLoppKP { get; set; }
        public Option<string> Resolutsioon { get; set; }
        public Option<DateTime?> SaabumiseKP { get; set; }
        public Option<DateTime?> SaabumiseKPVahemikuLoppKP { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<DateTime?> SeisundKPVahemikuLoppKP { get; set; }
        public Option<IList<ToimingOtsing>> SeotudToimingud { get; set; }
        public Option<string> Sissejuhatus { get; set; }
        public Option<string> Sisu { get; set; }
        public Option<long?> StaatusKL { get; set; }
        public Option<DateTime?> StaatusKP { get; set; }
        public Option<DateTime?> StaatusKPVahemikuLoppKP { get; set; }
        public Option<DateTime?> TahtaegKP { get; set; }
        public Option<DateTime?> TahtaegKPVahemikuLoppKP { get; set; }
        public Option<IList<AadressOtsing>> Tegevuskohad { get; set; }
        public Option<string> TeieAsjaajamiseNR { get; set; }
        public Option<IList<ToiminguOsalineOtsing>> ToiminguMenetlejad { get; set; }
        public Option<string> ToiminguNR { get; set; }
        public Option<IList<ToiminguOsalineOtsing>> ToiminguOsalised { get; set; }
        public Option<long?> ToiminguPohiFailiObjektID { get; set; }
        public Option<IList<JuriidilineIsikOtsing>> ToimingutMenetlevAsutus { get; set; }
        public Option<JuriidilineIsikOtsing> UusMenetlevAsutus { get; set; }
        public Option<string> Vabatekst { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}