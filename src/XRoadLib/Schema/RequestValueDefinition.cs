using System.Reflection;

namespace XRoadLib.Schema
{
    public class RequestValueDefinition : ContentDefinition
    {
        public OperationDefinition DeclaringOperationDefinition { get; }
        public ParameterInfo ParameterInfo { get; }

        public bool MergeContent { get; set; }

        public override string RuntimeName => "request";

        public RequestValueDefinition(ParameterInfo parameterInfo, OperationDefinition declaringOperationDefinition)
        {
            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = parameterInfo;

            if (parameterInfo == null)
                return;

            RuntimeType = NormalizeType(parameterInfo.ParameterType);

            InitializeContentDefinition(parameterInfo);
        }

        public override string ToString()
        {
            return $"Input value of {ParameterInfo.Member.DeclaringType?.FullName ?? "<null>"}.{ParameterInfo.Member.Name} ({Name})";
        }
    }
}