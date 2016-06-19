using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public abstract class Subject : XRoadSerializable
    {
        public string Name { get; set; }
    }
}