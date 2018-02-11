using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class Service1Request : XRoadSerializable
    {
        [XRoadXmlElement(Order = 1, IsOptional = true)]
        public ParamType1 Param1 { get; set; }

        [XRoadXmlElement(Order = 2, IsOptional = true)]
        public ParamType2 Param2 { get; set; }

        [XRoadXmlElement(Order = 3, IsOptional = true)]
        public ParamType3 Param3 { get; set; }
    }
}
