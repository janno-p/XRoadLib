using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrOtsus : IXRoadXmlSerializable
    {
        public Option<DateTime> AlgusKP { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<DateTime> JoustumiseKP { get; set; }

        public class KohtumaarusedType : IXRoadXmlSerializable
        {
            public Option<KarrOtsus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<KohtumaarusedType> Kohtumaarused { get; set; }
        public Option<long> LiikKL { get; set; }

        public class LopetatudKaristusedType : IXRoadXmlSerializable
        {
            public Option<KarrKaristus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<LopetatudKaristusedType> LopetatudKaristused { get; set; }
        public Option<string> MenetluseNR { get; set; }

        public class MoistetudKaristusedType : IXRoadXmlSerializable
        {
            public Option<KarrKaristus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MoistetudKaristusedType> MoistetudKaristused { get; set; }

        public class MuudKohtumaarusedInfoType : IXRoadXmlSerializable
        {
            public Option<string> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MuudKohtumaarusedInfoType> MuudKohtumaarusedInfo { get; set; }

        public class MuudKohtuotsusedInfoType : IXRoadXmlSerializable
        {
            public Option<string> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MuudKohtuotsusedInfoType> MuudKohtuotsusedInfo { get; set; }
        public Option<long> ObjektID { get; set; }
        public Option<string> OtsuseNR { get; set; }
        public Option<string> OtsuseTegija { get; set; }

        public class SeotudOtsusedObjektIDType : IXRoadXmlSerializable
        {
            public Option<long> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeotudOtsusedObjektIDType> SeotudOtsusedObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}