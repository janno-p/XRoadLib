using System;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract.Mapping
{
    [XmlType(AnonymousType = true)]
    public class WrapperType : XRoadSerializable
    {
        [XmlElement("Integer")]
        [XRoadOptional]
        public int[] Integers { get; set; }

        [XmlElement("String")]
        [XRoadOptional]
        public string[] Strings { get; set; }
    }

    public class MergeArrayContentRequest : XRoadSerializable
    {
        [XmlElement(Order = 1, DataType = "date")]
        public DateTime StartDate { get; set; }

        [XmlElement(Order = 2, DataType = "date")]
        public DateTime EndDate { get; set; }

        [XmlElement(Order = 3)]
        public WrapperType[] Content { get; set; }
    }

    public interface IMergeArrayContentService
    {
        [XRoadService("MergeArrayContent")]
        void Service(MergeArrayContentRequest request);
    }
}