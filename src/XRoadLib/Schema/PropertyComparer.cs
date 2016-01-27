namespace XRoadLib.Schema
{
    public class PropertyComparer : ContentComparer<ParameterDefinition>
    {
        public static PropertyComparer Instance { get; } = new PropertyComparer();

        private PropertyComparer()
        { }
    }
}