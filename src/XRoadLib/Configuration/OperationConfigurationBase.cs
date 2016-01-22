using System.Reflection;

namespace XRoadLib.Configuration
{
    public class OperationConfigurationBase : IOperationConfiguration
    {
        public virtual XRoadContentLayoutMode GetParameterLayout(MethodInfo methodInfo)
        {
            return XRoadContentLayoutMode.Strict;
        }

        public virtual bool HasHiddenXRoadFault(MethodInfo methodInfo)
        {
            return false;
        }

        public virtual string GetParameterName(ParameterInfo parameterInfo)
        {
            return null;
        }
    }
}