using JetBrains.Annotations;

namespace XRoadLib.Tests.Contract;

public enum Gender
{
    [UsedImplicitly] Unknown,
    Female,
    [UsedImplicitly] Male
}