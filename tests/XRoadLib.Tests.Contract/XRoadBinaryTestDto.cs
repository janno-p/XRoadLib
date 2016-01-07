using System.IO;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class XRoadBinaryTestDto : XRoadSerializable
    {
        public Stream Sisu { get; set; }
    }
}