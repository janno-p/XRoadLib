using System.IO;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    public class MimeContent : XRoadSerializable
    {
        public Stream Value { get; set; }
    }
}