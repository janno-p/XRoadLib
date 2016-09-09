using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KarrJareleparimine : IXRoadXmlSerializable
    {
        public Option<long> FailiObjektID { get; set; }
        public Option<string> Infosysteem { get; set; }
        public Option<Isik> Jareleparija { get; set; }
        public Option<boolean> OnValideeritud { get; set; }
        public Option<long> ParinguEesmarkKL { get; set; }
        public Option<int> ParinguID { get; set; }
        public Option<long> ParinguLiikKL { get; set; }
        public Option<string> Pohjendus { get; set; }
        public Option<DateTime> TeostamiseAeg { get; set; }
        public Option<DateTime> ValitudTeostamiseAeg { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}