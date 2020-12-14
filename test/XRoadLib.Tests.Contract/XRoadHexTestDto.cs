using System.IO;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class XRoadHexTestDto
    {
        [XRoadXmlElement(DataType = "hexBinary", UseXop = false)]
        public Stream Sisu { get; set; }
    }
}