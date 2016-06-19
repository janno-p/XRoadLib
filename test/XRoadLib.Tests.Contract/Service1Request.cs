using System.Xml.Serialization;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class Service1Request : XRoadSerializable
    {
        [XmlElement(Order = 1)]
        public ParamType1 Param1 { get; set; }

        [XmlElement(Order = 2)]
        public ParamType2 Param2 { get; set; }

        [XmlElement(Order = 3)]
        public ParamType3 Param3 { get; set; }
    }
}
