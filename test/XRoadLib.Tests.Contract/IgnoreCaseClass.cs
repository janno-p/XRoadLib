using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class IgnoreCaseClass
    {
        public long[] Objektid { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ObjektID { get; set; }
    }
}