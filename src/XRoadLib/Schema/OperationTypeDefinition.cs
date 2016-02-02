using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class OperationTypeDefinition
    {
        public MethodInfo MethodInfo { get; }

        public bool HasStrictContentOrder { get; set; }

        public IComparer<ParameterDefinition> ContentComparer { get; set; }

        public XName InputName { get; set; }

        public XName OutputName { get; set; }

        public string Documentation { get; set; }

        public OperationTypeDefinition(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }
    }
}