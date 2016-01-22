using System.Reflection;
using XRoadLib.Configuration;

namespace XRoadLib.Extensions
{
    internal static class PropertyInfoExtensions
    {
        internal static string GetPropertyName(this PropertyInfo propertyInfo, ITypeConfiguration typeConfiguration)
        {
            var customName = typeConfiguration?.GetPropertyName(propertyInfo);
            if (!string.IsNullOrWhiteSpace(customName))
                return customName;

            var elementName = propertyInfo.GetElementName();
            if (!string.IsNullOrWhiteSpace(elementName))
                return elementName;

            var start = propertyInfo.Name.LastIndexOf('.');

            return start >= 0 ? propertyInfo.Name.Substring(start + 1) : propertyInfo.Name;
        }
    }
}