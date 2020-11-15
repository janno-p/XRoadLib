using System.IO;
using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class XRoadBinaryTestDto : XRoadSerializable
    {
        [XRoadXmlElement(UseXop = false)]
        public Stream Sisu { get; set; }
    }
}