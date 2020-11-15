using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class IgnoreCaseClass : XRoadSerializable
    {
        public long[] Objektid { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ObjektID { get; set; }
    }
}
