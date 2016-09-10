using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrOtsus : IXRoadXmlSerializable
    {
        public Option<DateTime?> AlgusKP { get; set; }
        public Option<string> AsjaNR { get; set; }
        public Option<DateTime?> JoustumiseKP { get; set; }
        public Option<IList<KarrOtsus>> Kohtumaarused { get; set; }
        public Option<long?> LiikKL { get; set; }
        public Option<IList<KarrKaristus>> LopetatudKaristused { get; set; }
        public Option<string> MenetluseNR { get; set; }
        public Option<IList<KarrKaristus>> MoistetudKaristused { get; set; }
        public Option<IList<string>> MuudKohtumaarusedInfo { get; set; }
        public Option<IList<string>> MuudKohtuotsusedInfo { get; set; }
        public Option<long?> ObjektID { get; set; }
        public Option<string> OtsuseNR { get; set; }
        public Option<string> OtsuseTegija { get; set; }
        public Option<IList<long>> SeotudOtsusedObjektID { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}