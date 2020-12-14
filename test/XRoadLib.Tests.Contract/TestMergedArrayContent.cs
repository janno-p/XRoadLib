using System.Collections.Generic;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    [XRoadOperation("Service3")]
    public class TestMergedArrayContent : IXRoadRequest<int>, ITrackSpecifiedMembers
    {
        [XmlElement(Order = 1)]
        public string Value { get; set; }

        [XmlArray(Order = 2)]
        [XmlArrayItem("Code")]
        [XRoadMergeContent]
        public string[] Codes { get; set; }

        [XmlElement(Order = 3)]
        public string Value2 { get; set; }

        IDictionary<string, bool> ITrackSpecifiedMembers.SpecifiedMembers { get; set; }
    }
}