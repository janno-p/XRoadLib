using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public abstract class Subject
    {
        public string Name { get; set; }
    }
}