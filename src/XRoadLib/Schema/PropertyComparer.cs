namespace XRoadLib.Schema
{
    public class PropertyComparer : ContentComparer<PropertyDefinition>
    {
        public static PropertyComparer Instance { get; } = new PropertyComparer();

        private PropertyComparer()
        { }
    }
}