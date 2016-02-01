using System.Reflection;

namespace XRoadLib.Schema
{
    public class ParameterDefinition : ContentDefinition
    {
        public OperationDefinition Owner { get; }

        public ParameterInfo ParameterInfo { get; set; }

        public bool IsResult { get; set; }

        public ParameterDefinition(OperationDefinition owner)
        {
            Owner = owner;
        }

        public override string ContainerName => $"{ParameterInfo.Member.DeclaringType?.FullName}.{ParameterInfo.Member.Name}";
        public override string RuntimeName => ParameterInfo.Name;
    }
}