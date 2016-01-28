using System.Reflection;

namespace XRoadLib.Schema
{
    public class ParameterComparer : ContentComparer<ParameterInfo, ParameterDefinition>
    {
        public static ParameterComparer Instance { get; } = new ParameterComparer();

        private ParameterComparer()
        { }
    }
}