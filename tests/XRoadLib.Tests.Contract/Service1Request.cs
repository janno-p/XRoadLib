using System.Xml.Serialization;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class Service1Request : XRoadSerializable
    {
        [XmlElement(Order = 1)]
        public ParamType1 param1 { get; set; }

        [XmlElement(Order = 2)]
        public ParamType2 param2 { get; set; }

        [XmlElement(Order = 3)]
        public ParamType3 param3 { get; set; }
    }
}
