using System.Reflection;
using XRoadLib.Configuration;

namespace XRoadLib.Extensions
{
    internal static class ParameterInfoExtensions
    {
        internal static string GetParameterName(this ParameterInfo parameterInfo, IOperationConfiguration operationConfiguration)
        {
            var customName = operationConfiguration?.GetParameterName(parameterInfo);
            if (parameterInfo.Position < 0)
                return customName ?? "value";

            if (!string.IsNullOrWhiteSpace(customName))
                return customName;

            return parameterInfo.GetElementName() ?? parameterInfo.Name;
        }
    }
}