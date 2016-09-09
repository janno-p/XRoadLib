using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class KOLAMenetlus : IXRoadXmlSerializable
    {
        public Option<string> Alustatud { get; set; }
        public Option<string> Kohtunik { get; set; }
        public Option<string> Kohus { get; set; }
        public Option<DateTime?> LahendiKP { get; set; }
        public Option<string> MenetlusTeaveNimetus { get; set; }
        public Option<string> MenetlusTeaveURL { get; set; }
        public Option<string> TeiseAstmeStaatus { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}