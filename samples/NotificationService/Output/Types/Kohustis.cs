using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class Kohustis : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> Alustaja { get; set; }

        public class EnnistatavadKohustisedType : IXRoadXmlSerializable
        {
            public Option<Kohustis> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<EnnistatavadKohustisedType> EnnistatavadKohustised { get; set; }

        public class HoiatusedType : IXRoadXmlSerializable
        {
            public Option<ETHoiatus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<HoiatusedType> Hoiatused { get; set; }
        public Option<string> KlientsysteemiID { get; set; }
        public Option<Toiming> KohustiseMaaranudToiming { get; set; }
        public Option<Toiming> KohustistViimatiMojutanudToiming { get; set; }

        public class LaekumisedType : IXRoadXmlSerializable
        {
            public Option<Laekumine> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LaekumisedType> Laekumised { get; set; }
        public Option<string> Lopetaja { get; set; }

        public class LopetatavadKohustisedType : IXRoadXmlSerializable
        {
            public Option<Kohustis> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LopetatavadKohustisedType> LopetatavadKohustised { get; set; }
        public Option<DateTime> LoppKP { get; set; }

        public class MaksegraafikType : IXRoadXmlSerializable
        {
            public Option<OsaMakse> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MaksegraafikType> Maksegraafik { get; set; }
        public Option<boolean> MaksegraafikOnTyhistatav { get; set; }

        public class MakseRekvisiididType : IXRoadXmlSerializable
        {
            public Option<MakseRekvisiidid> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MakseRekvisiididType> MakseRekvisiidid { get; set; }
        public Option<decimal> MoistetudOriginaalSumma { get; set; }
        public Option<long> MoistetudOriginaalSummaValuutaKL { get; set; }
        public Option<decimal> MoistetudSumma { get; set; }
        public Option<long> MoistetudSummaValuutaKL { get; set; }
        public Option<string> Muutja { get; set; }
        public Option<DateTime> MuutmiseKP { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<Osaline> Osaline { get; set; }

        public class OsaMaksedType : IXRoadXmlSerializable
        {
            public Option<OsaMakse> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<OsaMaksedType> OsaMaksed { get; set; }
        public Option<long> RakendamiseLoppAlusKL { get; set; }
        public Option<string> RakendamiseLoppPohjendus { get; set; }
        public Option<DateTime> RakendamiseTahtaegKP { get; set; }
        public Option<DateTime> RakendumiseAlgusKP { get; set; }
        public Option<DateTime> RakendumiseLoppKP { get; set; }

        public class SeisundidKLType : IXRoadXmlSerializable
        {
            public Option<Olek> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeisundidKLType> SeisundidKL { get; set; }
        public Option<long> SeisundKL { get; set; }
        public Option<DateTime> SeisundKP { get; set; }
        public Option<string> Selgitus { get; set; }
        public Option<string> Sisestaja { get; set; }
        public Option<DateTime> SisestamiseKP { get; set; }
        public Option<DateTime> SulgemiseKP { get; set; }

        public class TagasimaksedType : IXRoadXmlSerializable
        {
            public Option<Tagasimakse> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<TagasimaksedType> Tagasimaksed { get; set; }
        public Option<boolean> Taidetud { get; set; }
        public Option<long> TaitmiseTapsustusKL { get; set; }
        public Option<decimal> TaodeldavSumma { get; set; }
        public Option<long> TaodeldavSummaValuutaKL { get; set; }
        public Option<decimal> TasutudSumma { get; set; }
        public Option<long> TasutudSummaValuutaKL { get; set; }
        public Option<boolean> Tyhistatud { get; set; }
        public Option<boolean> Vabastatud { get; set; }

        public class VastutajaType : IXRoadXmlSerializable
        {
            public Option<Osaline> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<VastutajaType> Vastutaja { get; set; }
        public Option<DateTime> VastutamiseAlgusKP { get; set; }
        public Option<DateTime> VastutamiseLoppKP { get; set; }
        public Option<long> VersID { get; set; }
        public Option<boolean> VoibKandaOsiti { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}