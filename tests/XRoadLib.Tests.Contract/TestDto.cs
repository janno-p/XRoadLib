using System;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class TestDto : XRoadSerializable
    {
        public string Nimi { get; set; }
        public string Kood { get; set; }
        public DateTime Loodud { get; set; }
    }
}