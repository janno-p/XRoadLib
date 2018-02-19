using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class IgnoreCaseClass : XRoadSerializable
    {
        public long[] Objektid { get; set; }
        public long ObjektID { get; set; }
    }
}
