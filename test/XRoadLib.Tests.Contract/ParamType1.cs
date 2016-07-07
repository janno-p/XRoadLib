using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class ParamType1 : XRoadSerializable
    {
        public long Property1 { get; set; }

        [XRoadOptional]
        public ParamType2[] Property2 { get; set; }

        [XRoadOptional]
        public string Property3 { get; set; }

        [XRoadOptional]
        public MimeContent MimeContent { get; set; }
    }
}