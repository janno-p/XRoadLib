using System.IO;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class MimeContent
    {
        [XRoadXmlElement(UseXop = false)]
        public Stream Value { get; set; }
    }
}