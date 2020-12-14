using System;
using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract.Mapping
{
    [XRoadSerializable]
    [XmlType(AnonymousType = true)]
    public class WrapperType
    {
        [XmlArrayItem("Integer")]
        [XRoadMergeContent]
        public int[] Integers { get; set; }

        [XmlArrayItem("String")]
        [XRoadMergeContent]
        public string[] Strings { get; set; }
    }

    [XRoadSerializable]
    public class MergeArrayContentRequest
    {
        [XmlElement(Order = 1, DataType = "date")]
        public DateTime StartDate { get; set; }

        [XmlElement(Order = 2, DataType = "date")]
        public DateTime EndDate { get; set; }

        [XmlArray(Order = 3)]
        [XRoadMergeContent]
        public WrapperType[] Content { get; set; }
    }

    public interface IMergeArrayContentService
    {
        [XRoadService("MergeArrayContent")]
        void Service(MergeArrayContentRequest request);
    }
}