using JetBrains.Annotations;

namespace XRoadLib.Tests.Contract;

[UsedImplicitly]
public class ParamType3 : XRoadSerializable
{
    public Subject? Subject { get; set; }
}