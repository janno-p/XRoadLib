using JetBrains.Annotations;

namespace XRoadLib.Tests.Contract;

[UsedImplicitly]
public class IgnoreCaseClass : XRoadSerializable
{
    [UsedImplicitly]
    public long[]? Objektid { get; set; }

    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public long ObjektID { get; set; }
}