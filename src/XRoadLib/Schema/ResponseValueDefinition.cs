using System.Reflection;

namespace XRoadLib.Schema
{
    public class ResponseValueDefinition : ContentDefinition
    {
        public OperationDefinition DeclaringOperationDefinition { get; }
        public ParameterInfo ParameterInfo { get; }

        public XRoadFaultPresentation XRoadFaultPresentation { get; set; } = XRoadFaultPresentation.Choice;

        public override string RuntimeName => "result";

        public ResponseValueDefinition(OperationDefinition declaringOperationDefinition)
        {
            var parameterInfo = declaringOperationDefinition.MethodInfo.ReturnParameter;

            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = parameterInfo;
            RuntimeType = NormalizeType(parameterInfo?.ParameterType);

            InitializeContentDefinition(parameterInfo);
        }

        public override string ToString()
        {
            return $"Return value of {ParameterInfo.Member.DeclaringType?.FullName ?? "<null>"}.{ParameterInfo.Member.Name} ({Name})";
        }
    }
}