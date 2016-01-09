using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [XRoadLayout(Layout = XRoadLayoutKind.All)]
    public class ParamType1 : XRoadSerializable
    {
        public long Property1 { get; set; }
        public ParamType2[] Property2 { get; set; }
        public string Property3 { get; set; }
        public MimeContent MimeContent { get; set; }
    }
}