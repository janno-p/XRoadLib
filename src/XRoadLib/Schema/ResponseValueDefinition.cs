using System.Reflection;

namespace XRoadLib.Schema
{
    public class ResponseValueDefinition : ContentDefinition
    {
        public OperationDefinition DeclaringOperationDefinition { get; }
        public ParameterInfo ParameterInfo { get; }

        public bool HasExplicitXRoadFault { get; set; }

        public override string RuntimeName => "result";

        public ResponseValueDefinition(OperationDefinition declaringOperationDefinition)
        {
            var parameterInfo = declaringOperationDefinition.MethodInfo.ReturnParameter;

            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = parameterInfo;
            RuntimeType = NormalizeType(parameterInfo?.ParameterType);
            HasExplicitXRoadFault = true;

            InitializeContentDefinition(parameterInfo);
        }

        public override string ToString()
        {
            return $"Return value of {ParameterInfo.Member.DeclaringType?.FullName ?? "<null>"}.{ParameterInfo.Member.Name} ({Name})";
        }
    }
}