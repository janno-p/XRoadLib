using System.Diagnostics.CodeAnalysis;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ParamType3 : XRoadSerializable
    {
        public Subject Subject { get; set; }
    }
}