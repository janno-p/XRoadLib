using System.Reflection;
using XRoadLib.Configuration;

namespace XRoadLib.Extensions
{
    internal static class ParameterInfoExtensions
    {
        internal static string GetParameterName(this ParameterInfo parameterInfo, IOperationConfiguration operationConfiguration)
        {
            var customName = operationConfiguration?.GetParameterName(parameterInfo);
            if (customName != null)
                return customName;

            return parameterInfo.GetElementName() ?? parameterInfo.Name.GetValueOrDefault("value");
        }
    }
}