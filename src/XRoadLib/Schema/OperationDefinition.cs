using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class OperationDefinition
    {
        public XName Name { get; set; }

        public BinaryContentMode BinaryContentMode { get; set; }

        public MethodInfo RuntimeMethod { get; set; }
    }
}