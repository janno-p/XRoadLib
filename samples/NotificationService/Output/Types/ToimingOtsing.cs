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
        public class AlaLiigiTapsustusKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlaLiigiTapsustusKLType> AlaLiigiTapsustusKL { get; set; }

        public class AlaLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlaLiikKLType> AlaLiikKL { get; set; }
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<DateTime?> AlgusKPVahemikuLoppKP { get; set; }
        public Option<long?> AllikaksOlevKlientSysteemKL { get; set; }

        public class AlusType : IXRoadXmlSerializable
        {
            public IList<SeaduseSateOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlusType> Alus { get; set; }

        public class AlusToimingudType : IXRoadXmlSerializable
        {
            public IList<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AlusToimingudType> AlusToimingud { get; set; }

        public class AnnotatsioonidType : IXRoadXmlSerializable
        {
            public IList<AnnotatsioonOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AnnotatsioonidType> Annotatsioonid { get; set; }

        public class AvalikustatudFailidType : IXRoadXmlSerializable
        {
            public IList<FailOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<AvalikustatudFailidType> AvalikustatudFailid { get; set; }

        public class ByrooToiminguosalisedType : IXRoadXmlSerializable
        {
            public IList<ToiminguOsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ByrooToiminguosalisedType> ByrooToiminguosalised { get; set; }
        public Option<DateTime?> DokumendiSaabumiseKP { get; set; }
        public Option<DateTime?> DokumendiSaabumiseKPVahemikuLoppKP { get; set; }
        public Option<string> EcliNR { get; set; }
        public Option<boolean> EdasiKaevatud { get; set; }
        public Option<long?> EdastamiseViisKL { get; set; }
        public Option<boolean> EriarvamusEsitatud { get; set; }
        public Option<long?> EsitamiseViisKL { get; set; }
        public Option<boolean> EXCLUDE { get; set; }

        public class FailidType : IXRoadXmlSerializable
        {
            public IList<FailOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<FailidType> Failid { get; set; }

        public class IstungisaalidType : IXRoadXmlSerializable
        {
            public IList<IstungisaalOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<IstungisaalidType> Istungisaalid { get; set; }
        public Option<DateTime?> JoustumisKP { get; set; }
        public Option<long?> KohustisObjektID { get; set; }

        public class KoikToiminguosalisedType : IXRoadXmlSerializable
        {
            public IList<ToiminguOsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KoikToiminguosalisedType> KoikToiminguosalised { get; set; }
        public Option<DateTime?> LabiviimiseAlgusKP { get; set; }
        public Option<DateTime?> LabiviimiseAlgusKPVahemikuLoppKP { get; set; }
        public Option<DateTime?> LabiviimiseLoppKP { get; set; }
        public Option<long?> LabiviimiseVormKL { get; set; }
        public Option<DateTime?> LahendiKuulutamiseAeg { get; set; }
        public Option<DateTime?> LahendiKuulutamiseAegVahemikuLoppKP { get; set; }

        public class LahenduseLiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LahenduseLiikKLType> LahenduseLiikKL { get; set; }

        public class LiikKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LiikKLType> LiikKL { get; set; }

        public class LisaToimingudType : IXRoadXmlSerializable
        {
            public IList<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LisaToimingudType> LisaToimingud { get; set; }

        public class MarksonastatusKLType : IXRoadXmlSerializable
        {
            public IList<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MarksonastatusKLType> MarksonastatusKL { get; set; }
        public Option<string> Markused { get; set; }
        public Option<string> MeieAsjaajamisNR { get; set; }
        public Option<MenetlusOtsing> Menetlus { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<long?> MenetluseObjektID { get; set; }

        public class MenetlusPostType : IXRoadXmlSerializable
        {
            public IList<MenetlusPostOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusPostType> MenetlusPost { get; set; }

        public class MojutatavadToimingudType : IXRoadXmlSerializable
        {
            public IList<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MojutatavadToimingudType> MojutatavadToimingud { get; set; }

        public class MojutavadToimingudType : IXRoadXmlSerializable
        {
            public IList<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MojutavadToimingudType> MojutavadToimingud { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<boolean> OnAvalikustatud { get; set; }
        public Option<FailOtsing> PohiFail { get; set; }

        public class PohiToimingudType : IXRoadXmlSerializable
        {
            public IList<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<PohiToimingudType> PohiToimingud { get; set; }
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

        public class SeotudToimingudType : IXRoadXmlSerializable
        {
            public IList<ToimingOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeotudToimingudType> SeotudToimingud { get; set; }
        public Option<string> Sissejuhatus { get; set; }
        public Option<string> Sisu { get; set; }
        public Option<long?> StaatusKL { get; set; }
        public Option<DateTime?> StaatusKP { get; set; }
        public Option<DateTime?> StaatusKPVahemikuLoppKP { get; set; }
        public Option<DateTime?> TahtaegKP { get; set; }
        public Option<DateTime?> TahtaegKPVahemikuLoppKP { get; set; }

        public class TegevuskohadType : IXRoadXmlSerializable
        {
            public IList<AadressOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TegevuskohadType> Tegevuskohad { get; set; }
        public Option<string> TeieAsjaajamiseNR { get; set; }

        public class ToiminguMenetlejadType : IXRoadXmlSerializable
        {
            public IList<ToiminguOsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToiminguMenetlejadType> ToiminguMenetlejad { get; set; }
        public Option<string> ToiminguNR { get; set; }

        public class ToiminguOsalisedType : IXRoadXmlSerializable
        {
            public IList<ToiminguOsalineOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToiminguOsalisedType> ToiminguOsalised { get; set; }
        public Option<long?> ToiminguPohiFailiObjektID { get; set; }

        public class ToimingutMenetlevAsutusType : IXRoadXmlSerializable
        {
            public IList<JuriidilineIsikOtsing> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<ToimingutMenetlevAsutusType> ToimingutMenetlevAsutus { get; set; }
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