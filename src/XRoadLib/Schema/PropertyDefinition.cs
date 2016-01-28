using System.Reflection;

namespace XRoadLib.Schema
{
    public class PropertyDefinition : ContentDefinition<PropertyInfo>
    {
        public TypeDefinition Owner { get; }

        public PropertyDefinition(TypeDefinition owner)
        {
            Owner = owner;
        }
    }
}