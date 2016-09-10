using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public abstract class Kohustis : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }
        public Option<IList<Kohustis>> EnnistatavadKohustised { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<Toiming> KohustiseMaaranudToiming { get; set; }
        public Option<Toiming> KohustistViimatiMojutanudToiming { get; set; }
        public Option<IList<Laekumine>> Laekumised { get; set; }
        public Option<string> Lopetaja { get; set; }
        public Option<IList<Kohustis>> LopetatavadKohustised { get; set; }
        public Option<DateTime?> LoppKP { get; set; }
        public Option<IList<OsaMakse>> Maksegraafik { get; set; }
        public Option<bool?> MaksegraafikOnTyhistatav { get; set; }
        public Option<IList<MakseRekvisiidid>> MakseRekvisiidid { get; set; }
        public Option<decimal?> MoistetudOriginaalSumma { get; set; }
        public Option<long?> MoistetudOriginaalSummaValuutaKL { get; set; }
        public Option<decimal?> MoistetudSumma { get; set; }
        public Option<long?> MoistetudSummaValuutaKL { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime?> MuutmiseKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<Osaline> Osaline { get; set; }
        public Option<IList<OsaMakse>> OsaMaksed { get; set; }
        public Option<long?> RakendamiseLoppAlusKL { get; set; }
        public Option<string> RakendamiseLoppPohjendus { get; set; }
        public Option<DateTime?> RakendamiseTahtaegKP { get; set; }
        public Option<DateTime?> RakendumiseAlgusKP { get; set; }
        public Option<DateTime?> RakendumiseLoppKP { get; set; }
        public Option<IList<Olek>> SeisundidKL { get; set; }
        public Option<long?> SeisundKL { get; set; }
        public Option<DateTime?> SeisundKP { get; set; }
        public Option<string> Selgitus { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime?> SisestamiseKP { get; set; }
        public Option<DateTime?> SulgemiseKP { get; set; }
        public Option<IList<Tagasimakse>> Tagasimaksed { get; set; }
        public Option<bool?> Taidetud { get; set; }
        public Option<long?> TaitmiseTapsustusKL { get; set; }
        public Option<decimal?> TaodeldavSumma { get; set; }
        public Option<long?> TaodeldavSummaValuutaKL { get; set; }
        public Option<decimal?> TasutudSumma { get; set; }
        public Option<long?> TasutudSummaValuutaKL { get; set; }
        public Option<bool?> Tyhistatud { get; set; }
        public Option<bool?> Vabastatud { get; set; }
        public Option<IList<Osaline>> Vastutaja { get; set; }
        public Option<DateTime?> VastutamiseAlgusKP { get; set; }
        public Option<DateTime?> VastutamiseLoppKP { get; set; }
        public Option<long> VersID { get; set; }
        public Option<bool?> VoibKandaOsiti { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}