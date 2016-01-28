using System.Reflection;

namespace XRoadLib.Schema
{
    public class PropertyComparer : ContentComparer<PropertyInfo, PropertyDefinition>
    {
        public static PropertyComparer Instance { get; } = new PropertyComparer();

        private PropertyComparer()
        { }
    }
}