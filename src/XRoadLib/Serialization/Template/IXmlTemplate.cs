namespace XRoadLib.Serialization.Template;

public interface IXmlTemplate
{
    [UsedImplicitly]
    IDictionary<string, Type> ParameterTypes { get; }

    [UsedImplicitly]
    IEnumerable<IXmlTemplateNode> ParameterNodes { get; }

    IXmlTemplateNode RequestNode { get; }

    IXmlTemplateNode ResponseNode { get; }
}