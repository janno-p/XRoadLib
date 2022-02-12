using System.Reflection;
using System.Xml.XPath;
using XRoadLib.Schema;

namespace XRoadLib.Serialization.Template;

public class XRoadXmlTemplate : IXmlTemplate
{
    private readonly XElement _requestNode;
    private readonly XElement _responseNode;
    private readonly IList<XElement> _parameterNodes;
    private readonly IList<string> _parameterNames;
    private readonly Dictionary<string, Type> _parameterTypes;

    public IDictionary<string, Type> ParameterTypes => _parameterTypes;

    public XRoadXmlTemplate(string templateXml, MethodInfo methodInfo)
    {
        if (!string.IsNullOrEmpty(templateXml))
        {
            var document = XDocument.Parse(templateXml);
            _requestNode = document.Root?.Elements("request").SingleOrDefault();
            _responseNode = document.Root?.Elements("response").SingleOrDefault();
        }

        _parameterNodes = _requestNode?.Elements().ToList() ?? new List<XElement>();

        if (methodInfo == null)
            return;

        _parameterNames = methodInfo.GetParameters().Select(parameter => parameter.Name).ToList();
        _parameterTypes = methodInfo.GetParameters().ToDictionary(param => param.Name, param => param.ParameterType);
    }

    [UsedImplicitly]
    public XRoadXmlTemplate() : this(null, null)
    { }

    public IXmlTemplateNode RequestNode => _requestNode != null ? new XRoadRequestTemplateNode(_requestNode.Name.LocalName, _requestNode) : EmptyNode;

    public IXmlTemplateNode ResponseNode
    {
        get
        {
            var elementNode = _responseNode?.Elements().SingleOrDefault();

            return elementNode != null ? new XRoadRequestTemplateNode(elementNode.Name.LocalName, elementNode) : EmptyNode;
        }
    }

    [UsedImplicitly]
    public IXmlTemplateNode GetParameterNode(string parameterName)
    {
        if (_requestNode == null)
            return EmptyNode;

        var index = _parameterNames.IndexOf(parameterName);
        if (index < 0 || _requestNode == null)
            return null;

        var documentNode = _parameterNodes[index];
        if (documentNode == null)
            throw new SchemaDefinitionException($"Service template does not define parameter named `{parameterName}`.");

        return new XRoadRequestTemplateNode(parameterName, documentNode);
    }

    public IEnumerable<IXmlTemplateNode> ParameterNodes => _parameterNames?.Select(GetParameterNode) ?? Enumerable.Empty<IXmlTemplateNode>();

    public static readonly IXmlTemplateNode EmptyNode = new XRoadRequestTemplateNode(string.Empty);

    private sealed class XRoadRequestTemplateNode : IXmlTemplateNode
    {
        private readonly XElement _node;

        public XRoadRequestTemplateNode(string name, XElement node = null)
        {
            Name = name;
            _node = node;
        }

        private static bool IsInRange(XAttribute versionAttribute, uint version)
        {
            if (versionAttribute == null)
                return true;

            var values = versionAttribute.Value.Split('-');
            var addedVersion = Convert.ToUInt32(values[0]);
            var removedVersion = values[1] == "*" ? uint.MaxValue : Convert.ToUInt32(values[1]);

            return version >= addedVersion && version < removedVersion;
        }

        public IXmlTemplateNode this[string childNodeName, uint version]
        {
            get
            {
                if (_node == null)
                    return this;

                var childNode = _node.Elements(childNodeName).SingleOrDefault(n => IsInRange(n.Attribute("version"), version));
                if (childNode == null)
                    return null;

                var refAttribute = childNode.Attributes("xpath-ref").SingleOrDefault();
                if (refAttribute == null)
                    return new XRoadRequestTemplateNode(childNodeName, childNode);

                var refNode = childNode.XPathSelectElements(refAttribute.Value).SingleOrDefault();

                return new XRoadRequestTemplateNode(childNodeName, refNode);
            }
        }

        public string Name { get; }

        public bool IsRequired
        {
            get
            {
                var requiredAttribute = _node?.Attributes("required").SingleOrDefault();
                return requiredAttribute is { Value: "true" };
            }
        }

        public IEnumerable<string> ChildNames { get { return _node?.Elements().Select(childNode => childNode.Name.LocalName) ?? Enumerable.Empty<string>(); } }

        public string Namespace => null;

        public int CountRequiredNodes(uint version)
        {
            if (_node == null)
                return 0;

            return _node.Elements()
                        .Where(n => IsInRange(n.Attribute("version"), version))
                        .Count(n => n.Attributes("required").Any(a => a.Value == "true"));
        }
    }
}