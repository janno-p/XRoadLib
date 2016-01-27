using System.Reflection;

namespace XRoadLib.Schema
{
    public class ParameterDefinition : ContentDefinition<ParameterInfo>
    {
        public OperationDefinition Owner { get; }

        public ParameterDefinition(OperationDefinition owner)
        {
            Owner = owner;
        }
    }
}