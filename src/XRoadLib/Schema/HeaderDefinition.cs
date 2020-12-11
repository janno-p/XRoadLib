using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Headers;

namespace XRoadLib.Schema
{
    public static class HeaderDefinition
    {
        public static IHeaderDefinition Default =>
            new HeaderDefinition<XRoadHeader>(nameof(IHeaderDefinition.RequiredHeaders))
                .AddRequiredHeader(x => x.Client)
                .AddRequiredHeader(x => x.Service)
                .AddRequiredHeader(x => x.UserId)
                .AddRequiredHeader(x => x.Id)
                .AddRequiredHeader(x => x.Issue)
                .AddRequiredHeader(x => x.ProtocolVersion)
                .AddHeaderNamespace(NamespaceConstants.XRoad)
                .AddHeaderNamespace(NamespaceConstants.XRoadRepr);
    }

    /// <summary>
    /// Configuration options of X-Road header.
    /// </summary>
    public class HeaderDefinition<THeader> : IHeaderDefinition where THeader : ISoapHeader, new()
    {
        private readonly ISet<string> _headerNamespaces = new HashSet<string>();

        /// <summary>
        /// Names of SOAP header elements required by service description.
        /// </summary>
        public ISet<XName> RequiredHeaders { get; } = new SortedSet<XName>(new XNameComparer());

        /// <summary>
        /// Name of WSDL message used to define SOAP header elements.
        /// </summary>
        public string MessageName { get; }

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        public bool IsHeaderNamespace(string namespaceName) =>
            _headerNamespaces.Contains(namespaceName);

        public HeaderDefinition(string messageName) =>
            MessageName = messageName;

        /// <summary>
        /// Creates new instance of definition specific SOAP header.
        /// </summary>
        /// <returns>Definition specific SOAP header</returns>
        public ISoapHeader CreateHeader() =>
            new THeader();

        /// <summary>
        /// Specify mandatory element of the header object.
        /// </summary>
        public HeaderDefinition<THeader> AddRequiredHeader<TValue>(Expression<Func<THeader, TValue>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
                throw new SchemaDefinitionException($"Only MemberExpression is allowed to use for SOAP header definition, but was {expression.Body.GetType().Name} ({GetType().Name}).");

            var attribute = memberExpression.Member.GetSingleAttribute<XmlElementAttribute>() ?? memberExpression.Member.DeclaringType.GetElementAttributeFromInterface(memberExpression.Member as PropertyInfo);
            if (string.IsNullOrWhiteSpace(attribute?.ElementName))
                throw new SchemaDefinitionException($"Specified member `{memberExpression.Member.Name}` does not define any XML element.");

            RequiredHeaders.Add(XName.Get(attribute.ElementName, attribute.Namespace));

            return this;
        }

        /// <summary>
        /// Define namespaces used to define SOAP header elements.
        /// </summary>
        public HeaderDefinition<THeader> AddHeaderNamespace(string namespaceName)
        {
            _headerNamespaces.Add(namespaceName);

            return this;
        }

        private class XNameComparer : IComparer<XName>
        {
            public int Compare(XName x, XName y)
            {
                var ns = string.Compare(x?.NamespaceName ?? "", y?.NamespaceName ?? "", StringComparison.OrdinalIgnoreCase);
                return ns != 0 ? ns : string.Compare(x?.LocalName ?? "", y?.LocalName ?? "", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}