using System.IO;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class XRoadBinaryTestDto
    {
        [XRoadXmlElement(UseXop = false)]
        public Stream Sisu { get; set; }
    }
}