using JetBrains.Annotations;

namespace XRoadLib.Tests.Contract;

[UsedImplicitly]
public abstract class Subject : XRoadSerializable
{
    public string? Name { get; set; }
}