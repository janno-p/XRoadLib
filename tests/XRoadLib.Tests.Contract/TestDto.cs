using System;
using XRoadLib.Attributes;
using XRoadLib.Serialization;
using XRoadLib.Tests.Contract.Attributes;
using XRoadLib.Tests.Contract.Comparers;

namespace XRoadLib.Tests.Contract
{
    [XRoadLayout(ContentLayout = XRoadContentLayoutMode.Strict, Comparer = typeof(OrderComparer))]
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