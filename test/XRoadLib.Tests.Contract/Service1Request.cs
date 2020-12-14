using System.Collections.Generic;
using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    [XRoadOperation("Service1")]
    public class Service1Request : IXRoadRequest<int>, ITrackSpecifiedMembers
    {
        [XRoadXmlElement(Order = 1, IsOptional = true)]
        public ParamType1 Param1 { get; set; }

        [XRoadXmlElement(Order = 2, IsOptional = true)]
        public ParamType2 Param2 { get; set; }

        [XRoadXmlElement(Order = 3, IsOptional = true)]
        public ParamType3 Param3 { get; set; }

        IDictionary<string, bool> ITrackSpecifiedMembers.SpecifiedMembers { get; set; }
    }
}