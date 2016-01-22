using System.Reflection;

namespace XRoadLib.Configuration
{
    public interface IOperationConfiguration
    {
        /// <summary>
        /// Specifies parameter order mode for operation root types:
        /// `Flexible` mode uses XML Schema `all` element.
        /// `Strict` mode uses XML Schema `sequence` element.
        /// </summary>
        XRoadContentLayoutMode GetParameterLayout(MethodInfo methodInfo);

        /// <summary>
        /// Specifies that method should not define explicit X-Road faults in service description.
        /// </summary>
        bool HasHiddenXRoadFault(MethodInfo methodInfo);

        /// <summary>
        /// Specifies alternative names for operation parameters.
        /// </summary>
        string GetParameterName(ParameterInfo parameterInfo);
    }
}