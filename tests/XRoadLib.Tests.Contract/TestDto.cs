using System;
using XRoadLib.Serialization;
using XRoadLib.Tests.Contract.Attributes;

namespace XRoadLib.Tests.Contract
{
    public class TestDto : XRoadSerializable
    {
        [Order(1)]
        public string Nimi { get; set; }

        [Order(2)]
        public string Kood { get; set; }

        [Order(3)]
        public DateTime Loodud { get; set; }
    }
}