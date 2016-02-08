using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    public class OperationDefinition : Definition
    {
        public MethodInfo MethodInfo { get; }

        public uint Version { get; set; }
        public bool IsAbstract { get; set; }

        public string InputMessageName { get; set; }
        public BinaryMode InputBinaryMode { get; set; }
        public bool ProhibitRequestPartInResponse { get; set; }

        public string OutputMessageName { get; set; }
        public BinaryMode OutputBinaryMode { get; set; }

        public OperationDefinition(XName qualifiedName, uint? version, MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;

            var serviceAttribute = methodInfo.GetServices().SingleOrDefault(x => x.Name == qualifiedName.LocalName);

            Name = qualifiedName;
            IsAbstract = (serviceAttribute?.IsAbstract).GetValueOrDefault();
            InputBinaryMode = BinaryMode.Xml;
            OutputBinaryMode = BinaryMode.Xml;
            State = (serviceAttribute?.IsHidden).GetValueOrDefault() ? DefinitionState.Hidden : DefinitionState.Default;
            Version = version.GetValueOrDefault(serviceAttribute?.AddedInVersion ?? 1u);
            ProhibitRequestPartInResponse = false;
            InputMessageName = qualifiedName.LocalName;
            OutputMessageName = $"{qualifiedName.LocalName}Response";
            Documentation = methodInfo.GetXRoadTitles().Where(title => !string.IsNullOrWhiteSpace(title.Item2)).ToArray();
        }
    }
}