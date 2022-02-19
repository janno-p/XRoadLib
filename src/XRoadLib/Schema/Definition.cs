namespace XRoadLib.Schema;

public abstract class Definition
{
    public XName? Name { get; set; }

    public DefinitionState State { get; set; }

    [UsedImplicitly]
    public DocumentationDefinition Documentation { get; set; }

    public Tuple<XName, string>[] CustomAttributes { get; set; }
}