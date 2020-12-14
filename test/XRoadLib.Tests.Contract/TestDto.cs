using System;
using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class TestDto
    {
        [XmlElement(Order = 1)]
        public string Nimi { get; set; }

        [XmlElement(Order = 2)]
        public string Kood { get; set; }

        [XmlElement(Order = 3)]
        public DateTime Loodud { get; set; }
    }
}