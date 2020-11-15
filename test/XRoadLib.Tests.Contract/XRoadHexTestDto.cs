using System.IO;
using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class XRoadHexTestDto : XRoadSerializable
    {
        [XRoadXmlElement(DataType = "hexBinary", UseXop = false)]
        public Stream Sisu { get; set; }
    }
}