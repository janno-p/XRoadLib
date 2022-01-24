using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Tests.Contract;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class IgnoreCaseClass : XRoadSerializable
{
    public long[]? Objektid { get; set; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public long ObjektID { get; set; }
}