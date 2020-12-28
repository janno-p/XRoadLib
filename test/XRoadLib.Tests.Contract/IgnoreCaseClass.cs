using XRoadLib.Attributes;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class IgnoreCaseClass
    {
        public long[] Objektid { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ObjektID { get; set; }
    }
    
    [XRoadOperation]
    public class Service4 : XRoadOperation<IgnoreCaseClass, UnitResponse, XRoadHeader> { }
}