using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XRoadLib.Serialization.Template
{
    public class XRoadXmlTemplate : IXmlTemplate
    {
        private readonly XElement requestNode;
        private readonly XElement responseNode;
        private readonly IList<XElement> parameterNodes;
        private readonly IList<string> parameterNames;
        private readonly Dictionary<string, Type> parameterTypes;

        public IDictionary<string, Type> ParameterTypes => parameterTypes;

        public XRoadXmlTemplate(string templateXml, MethodInfo methodInfo)
        {
            if (!string.IsNullOrEmpty(templateXml))
            {
                var document = XDocument.Parse(templateXml);
                requestNode = document.Root?.Elements("request").SingleOrDefault();
                responseNode = document.Root?.Elements("response").SingleOrDefault();
            }

            parameterNodes = requestNode?.Elements().ToList() ?? new List<XElement>();

            if (methodInfo == null)
                return;

            parameterNames = methodInfo.GetParameters().Select(parameter => parameter.Name).ToList();
            parameterTypes = methodInfo.GetParameters().ToDictionary(param => param.Name, param => param.ParameterType);
        }

        public XRoadXmlTemplate() : this(null, null)
        { }

        public IXmlTemplateNode RequestNode => requestNode != null ? new XRoadRequestTemplateNode(requestNode.Name.LocalName, requestNode) : EmptyNode;

        public IXmlTemplateNode ResponseNode
        {
            get
            {
                var elementNode = responseNode?.Elements().SingleOrDefault();

                return elementNode != null ? new XRoadRequestTemplateNode(elementNode.Name.LocalName, elementNode) : EmptyNode;
            }
        }

        public IXmlTemplateNode GetParameterNode(string parameterName)
        {
            if (requestNode == null)
                return EmptyNode;

            var index = parameterNames.IndexOf(parameterName);
            if (index < 0 || requestNode == null)
                return null;

            var documentNode = parameterNodes[index];
            if (documentNode == null)
                throw XRoadException.ParameterUndefinedInTemplate(parameterName);

            return new XRoadRequestTemplateNode(parameterName, documentNode);
        }

        public IEnumerable<IXmlTemplateNode> ParameterNodes => parameterNames?.Select(GetParameterNode) ?? Enumerable.Empty<IXmlTemplateNode>();

        public static readonly IXmlTemplateNode EmptyNode = new XRoadRequestTemplateNode(string.Empty);

        #region Nested type: XteeXmlValidatorNode

        private class XRoadRequestTemplateNode : IXmlTemplateNode
        {
            private readonly XElement node;

            public XRoadRequestTemplateNode(string name, XElement node = null)
            {
                Name = name;
                this.node = node;
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
                    if (node == null)
                        return this;

                    var childNode = node.Elements(childNodeName).SingleOrDefault(n => IsInRange(n.Attribute("version"), version));
                    if (childNode == null)
                        return null;

                    var refAttribute = childNode.Attributes("xpath-ref").SingleOrDefault();
                    if (refAttribute == null)
                        return new XRoadRequestTemplateNode(childNodeName, childNode);

#if NETSTANDARD1_5
                    var refNode = (XElement)XNode.ReadFrom(childNode.CreateNavigator().SelectSingleNode(refAttribute.Value).ReadSubtree());
#else
                    var refNode = childNode.XPathSelectElements(refAttribute.Value).SingleOrDefault();
#endif

                    return new XRoadRequestTemplateNode(childNodeName, refNode);
                }
            }

            public string Name { get; }

            public bool IsRequired
            {
                get
                {
                    var requiredAttribute = node?.Attributes("required").SingleOrDefault();
                    return requiredAttribute != null && requiredAttribute.Value == "true";
                }
            }

            public IEnumerable<string> ChildNames { get { return node?.Elements().Select(childNode => childNode.Name.LocalName) ?? Enumerable.Empty<string>(); } }

            public string Namespace => null;

            public int CountRequiredNodes(uint version)
            {
                if (node == null)
                    return 0;

                return node.Elements()
                           .Where(n => IsInRange(n.Attribute("version"), version))
                           .Count(n => n.Attributes("required").Any(a => a.Value == "true"));
            }
        }

        #endregion Nested type: XteeXmlValidatorNode
    }
}