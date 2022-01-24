using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Tests.Contract;

public enum Gender
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")] Unknown,
    Female,
    [SuppressMessage("ReSharper", "UnusedMember.Global")] Male
}