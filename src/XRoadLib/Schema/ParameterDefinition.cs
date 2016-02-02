using System.Reflection;

namespace XRoadLib.Schema
{
    public class ParameterDefinition : ContentDefinition
    {
        public OperationTypeDefinition DeclaringOperationTypeDefinition { get; }

        public ParameterInfo ParameterInfo { get; }

        public bool IsResult { get; }

        public override string ContainerName => $"{ParameterInfo.Member.DeclaringType?.FullName}.{ParameterInfo.Member.Name}";
        public override string RuntimeName => ParameterInfo.Name;

        public ParameterDefinition(ParameterInfo parameterInfo, OperationTypeDefinition declaringOperationTypeDefinition)
        {
            DeclaringOperationTypeDefinition = declaringOperationTypeDefinition;
            ParameterInfo = parameterInfo;
            IsResult = parameterInfo.Position < 0;
        }
    }
}