using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class ParamType1 : XRoadSerializable
    {
        public long Property1 { get; set; }

        [XRoadXmlArray(IsOptional = true)]
        public ParamType2[] Property2 { get; set; }

        [XRoadXmlElement(IsOptional = true)]
        public string Property3 { get; set; }

        [XRoadXmlElement(IsOptional = true)]
        public MimeContent MimeContent { get; set; }
    }
}