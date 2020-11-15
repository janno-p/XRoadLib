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
    /// <summary>
    /// Allows to fluently configure headers mandatory elements.
    /// </summary>
    public interface IHeaderDefinitionBuilder<THeader> where THeader : IXRoadHeader
    {
        /// <summary>
        /// Specify mandatory element of the header object.
        /// </summary>
        IHeaderDefinitionBuilder<THeader> WithRequiredHeader<TValue>(Expression<Func<THeader, TValue>> expression);

        /// <summary>
        /// Define namespaces used to define SOAP header elements.
        /// </summary>
        IHeaderDefinitionBuilder<THeader> WithHeaderNamespace(string namespaceName);
    }

    /// <summary>
    /// Configuration options of X-Road header.
    /// </summary>
    public class HeaderDefinition
    {
        private readonly ISet<string> _headerNamespaces = new HashSet<string>();

        /// <summary>
        /// Names of SOAP header elements required by service description.
        /// </summary>
        public ISet<XName> RequiredHeaders { get; } = new SortedSet<XName>(new XNameComparer());

        /// <summary>
        /// Name of WSDL message used to define SOAP header elements.
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// Define custom header type for X-Road messages.
        /// </summary>
        public IHeaderDefinitionBuilder<THeader> Use<THeader>() where THeader : IXRoadHeader, new()
        {
            return new HeaderDefinitionBuilder<THeader>(this);
        }

        /// <summary>
        /// Remove SOAP header definition from message.
        /// </summary>
        public void Remove()
        {
            RequiredHeaders.Clear();
            _headerNamespaces.Clear();
        }

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        public bool IsHeaderNamespace(string namespaceName)
        {
            return _headerNamespaces.Contains(namespaceName);
        }

        private class HeaderDefinitionBuilder<THeader> : IHeaderDefinitionBuilder<THeader> where THeader : IXRoadHeader
        {
            private readonly HeaderDefinition _headerDefinition;

            public HeaderDefinitionBuilder(HeaderDefinition headerDefinition)
            {
                this._headerDefinition = headerDefinition;
                headerDefinition.Remove();
            }

            public IHeaderDefinitionBuilder<THeader> WithRequiredHeader<TValue>(Expression<Func<THeader, TValue>> expression)
            {
                if (!(expression.Body is MemberExpression memberExpression))
                    throw new SchemaDefinitionException($"Only MemberExpression is allowed to use for SOAP header definition, but was {expression.Body.GetType().Name} ({GetType().Name}).");

                var attribute = memberExpression.Member.GetSingleAttribute<XmlElementAttribute>() ?? memberExpression.Member.DeclaringType.GetElementAttributeFromInterface(memberExpression.Member as PropertyInfo);
                if (string.IsNullOrWhiteSpace(attribute?.ElementName))
                    throw new SchemaDefinitionException($"Specified member `{memberExpression.Member.Name}` does not define any XML element.");

                _headerDefinition.RequiredHeaders.Add(XName.Get(attribute.ElementName, attribute.Namespace));

                return this;
            }

            public IHeaderDefinitionBuilder<THeader> WithHeaderNamespace(string namespaceName)
            {
                _headerDefinition._headerNamespaces.Add(namespaceName);

                return this;
            }
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
