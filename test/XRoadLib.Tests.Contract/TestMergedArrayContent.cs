using System.Collections.Generic;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class TestMergedArrayContent : ITrackSpecifiedMembers
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
    
    [XRoadOperation]
    public class Service3 : XRoadOperation<TestMergedArrayContent, int, XRoadHeader>
    { }
}