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
        public Option<bool?> AvaldamiseleMittekuuluvAsi { get; set; }
        public Option<string> AvalikNimetus { get; set; }
        public Option<IList<Fail>> Failid { get; set; }
        public Option<decimal?> HagiHind { get; set; }
        public Option<long?> HagiHindValuutaKL { get; set; }
        public Option<bool?> HagiTagamineEsialgneOiguskaitse { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<int?> JrkNr { get; set; }
        public Option<bool?> KaebusOnEsitatud { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<Kohtuasi> Kohtuasi { get; set; }
        public Option<string> KOLANr { get; set; }
        public Option<MenetluseKontakt> Kontakt { get; set; }
        public Option<IList<Kulu>> Kulud { get; set; }
        public Option<DateTime?> LahendiTeatavakstegemiseAegKP { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<bool?> MenetlejaOnMaaramata { get; set; }
        public Option<DateTime?> MenetlemiseAlgusKP { get; set; }
        public Option<string> MenetlenudValisriigiAsutus { get; set; }
        public Option<IList<JuriidilineIsik>> MenetlevadAsutused { get; set; }
        public Option<string> MenetlevadAsutusedCSV { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<string> MenetluseNrKoosJrkNr { get; set; }
        public Option<IList<MenetluseSisulineLiigitus>> MenetluseSisulineLiigitus { get; set; }
        public Option<long?> MenetlusLiigiAlaLiikKL { get; set; }
        public Option<long?> MenetlusLiikKL { get; set; }
        public Option<IList<MenetlusPost>> MenetlusPost { get; set; }
        public Option<IList<Osaline>> MuudOsalised { get; set; }
        public Option<bool?> MuuJuurdepaasupiirang { get; set; }
        public Option<string> MuuJuurdepaasupiiranguPohjendus { get; set; }
        public Option<string> Nimetus { get; set; }
        public Option<IList<Noue>> Nouded { get; set; }
        public Option<IList<Objekt>> Objektid { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<bool?> OigusabiMenetlusabiSaamine { get; set; }
        public Option<IList<Osaline>> Osalised { get; set; }
        public Option<bool?> PuudutabAlaealisi { get; set; }
        public Option<DateTime?> RegistreerimiseKP { get; set; }
        public Option<long?> SalastatuseTaseKL { get; set; }
        public Option<IList<Sanktsioon>> Sanktsioonid { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<IList<SeotudMenetlus>> SeotudAsjad { get; set; }
        public Option<IList<SeotudMenetlus>> SeotudMenetlused { get; set; }
        public Option<IList<Syyteosyndmus>> Syyteosyndmused { get; set; }
        public Option<IList<Toiming>> Toimingud { get; set; }
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