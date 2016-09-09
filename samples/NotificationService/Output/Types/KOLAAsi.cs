using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KOLAAsi : IXRoadXmlSerializable
    {
        public Option<string> AlustatudKP { get; set; }
        public Option<string> AsjaKategooria { get; set; }
        public Option<int?> AsjaKood { get; set; }
        public Option<string> AsjaLiik { get; set; }
        public Option<string> AsjaNumber { get; set; }
        public Option<string> AstmeStaatus { get; set; }
        public Option<string> Kohtunik { get; set; }
        public Option<string> Kohus { get; set; }
        public Option<DateTime?> LahendiKP { get; set; }
        public Option<string> MenetlusaluneOsaline { get; set; }

        public class MenetlusedType : IXRoadXmlSerializable
        {
            public IList<KOLAMenetlus> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<MenetlusedType> Menetlused { get; set; }
        public Option<int?> MenetluseKood { get; set; }
        public Option<string> Osaline { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}