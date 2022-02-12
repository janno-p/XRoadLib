using System.Reflection;
using XRoadLib.Schema;

namespace XRoadLib.Extensions;

[UsedImplicitly]
internal static class MethodInfoExtensions
{
    [UsedImplicitly]
    internal static string GetOperationNameFromMethodInfo(this MethodInfo methodInfo)
    {
        if (methodInfo.DeclaringType == null)
            throw new SchemaDefinitionException($"Declaring type of method `{methodInfo.Name}` is not defined.");

        if (methodInfo.DeclaringType.Name.StartsWith("I") && methodInfo.DeclaringType.Name.Length > 1 && char.IsUpper(methodInfo.DeclaringType.Name[1]))
            return methodInfo.DeclaringType.Name.Substring(1);

        return methodInfo.DeclaringType.Name;
    }
}