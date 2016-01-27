namespace XRoadLib.Schema
{
    public class PropertyDefinition : ContentDefinition<PropertyDefinition>
    {
        public TypeDefinition Owner { get; }

        public PropertyDefinition(TypeDefinition owner)
        {
            Owner = owner;
        }
    }
}