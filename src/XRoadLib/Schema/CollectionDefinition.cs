namespace XRoadLib.Schema;

public class CollectionDefinition : TypeDefinition
{
    [UsedImplicitly]
    public TypeDefinition ItemDefinition { get; set; }

    public CollectionDefinition(Type type, TypeDefinition itemDefinition, string targetNamespace)
        : base(type, targetNamespace)
    {
        ItemDefinition = itemDefinition;
    }
}