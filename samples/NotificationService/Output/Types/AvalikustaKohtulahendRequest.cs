using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class AvalikustaKohtulahendRequest : IXRoadXmlSerializable
    {
        public Kohtutoiming Kohtutoiming { get; set; }
        public AvalikustamiseRekvisiidid AvalikustamiseRekvisiidid { get; set; }
        public Isik Kasutaja { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}