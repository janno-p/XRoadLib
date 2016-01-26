using System.Reflection;
using System.Xml.Serialization;
using XRoadLib.Configuration;

namespace XRoadLib.Extensions
{
    internal static class ParameterInfoExtensions
    {
        internal static string GetCustomizedParameterName(this ParameterInfo parameterInfo, IOperationConfiguration operationConfiguration)
        {
            var parameterName = operationConfiguration?.GetParameterName(parameterInfo);
            if (parameterName != null)
                return parameterName;

            parameterName = parameterInfo.GetElementName();
            if (!string.IsNullOrWhiteSpace(parameterName))
                return parameterName;

            return parameterInfo.GetSingleAttribute<XmlArrayAttribute>()?.ElementName;
        }

        internal static string GetParameterName(this ParameterInfo parameterInfo, IOperationConfiguration operationConfiguration)
        {
            return parameterInfo.GetCustomizedParameterName(operationConfiguration) ?? parameterInfo.Name.GetValueOrDefault("value");
        }
    }
}