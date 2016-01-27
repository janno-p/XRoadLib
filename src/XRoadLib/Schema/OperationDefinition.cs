using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class OperationDefinition : ContainerDefinition<MethodInfo, ParameterDefinition>
    {
        public XName RequestTypeName { get; set; }

        public XName ResponseTypeName { get; set; }

        public string RequestMessageName { get; set; }

        public string ResponseMessageName { get; set; }

        public BinaryContentMode BinaryContentMode { get; set; }

        public bool HideXRoadFaultDefinition { get; set; }

        public uint Version { get; set; }
    }
}