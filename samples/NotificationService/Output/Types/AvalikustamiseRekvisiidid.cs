using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class AvalikustamiseRekvisiidid : Ymbrik, IXRoadXmlSerializable
    {
        public Option<long?> AvalikustamiseMargeKL { get; set; }
        public Option<IList<Fail>> Failid { get; set; }
        public Option<IList<ETHoiatus>> Hoiatused { get; set; }
        public Option<string> Markused { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}