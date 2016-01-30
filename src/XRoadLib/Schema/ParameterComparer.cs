namespace XRoadLib.Schema
{
    public class ParameterComparer : ContentComparer<ParameterDefinition>
    {
        public static ParameterComparer Instance { get; } = new ParameterComparer();

        private ParameterComparer()
        { }
    }
}