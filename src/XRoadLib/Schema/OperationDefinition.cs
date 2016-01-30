using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class OperationDefinition : ContainerDefinition<ParameterDefinition>
    {
        public XName RequestTypeName { get; set; }

        public XName ResponseTypeName { get; set; }

        public string RequestMessageName { get; set; }

        public string ResponseMessageName { get; set; }

        public BinaryMode RequestBinaryMode { get; set; }

        public BinaryMode ResponseBinaryMode { get; set; }

        public bool HideXRoadFaultDefinition { get; set; }

        public bool ProhibitRequestPartInResponse { get; set; }

        public uint Version { get; set; }

        public MethodInfo MethodInfo { get; set; }
    }
}