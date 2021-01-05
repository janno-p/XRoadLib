using System.Diagnostics.CodeAnalysis;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class Subject : XRoadSerializable
    {
        public string Name { get; set; }
    }
}