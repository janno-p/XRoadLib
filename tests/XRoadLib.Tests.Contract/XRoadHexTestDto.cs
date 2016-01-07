using System.IO;
using System.Xml.Serialization;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class XRoadHexTestDto : XRoadSerializable
    {
        [XmlElement(DataType = "hexBinary")]
        public Stream Sisu { get; set; }
    }
}