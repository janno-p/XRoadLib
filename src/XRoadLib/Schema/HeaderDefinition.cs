using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Protocols.Headers;

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
    }

    /// <summary>
    /// Configuration options of X-Road header.
    /// </summary>
    public class HeaderDefinition
    {
        /// <summary>
        /// Create new instance of header object.
        /// </summary>
        public Func<IXRoadHeader> Initializer { get; private set; }

        /// <summary>
        /// Names of SOAP header elements required by service description.
        /// </summary>
        public ISet<XName> RequiredHeaders { get; } = new SortedSet<XName>(new XNameComparer());

        /// <summary>
        /// Define custom header type for X-Road messages.
        /// </summary>
        public IHeaderDefinitionBuilder<THeader> Use<THeader>(Func<THeader> initializer) where THeader : IXRoadHeader
        {
            Initializer = () => initializer();

            return new HeaderDefinitionBuilder<THeader>(this);
        }

        private class HeaderDefinitionBuilder<THeader> : IHeaderDefinitionBuilder<THeader> where THeader : IXRoadHeader
        {
            private readonly HeaderDefinition headerDefinition;

            public HeaderDefinitionBuilder(HeaderDefinition headerDefinition)
            {
                this.headerDefinition = headerDefinition;
                headerDefinition.RequiredHeaders.Clear();
            }

            public IHeaderDefinitionBuilder<THeader> WithRequiredHeader<TValue>(Expression<Func<THeader, TValue>> expression)
            {
                var memberExpression = expression.Body as MemberExpression;
                if (memberExpression == null)
                    throw new ArgumentException($"Only MemberExpression is allowed to use for SOAP header definition, but was {expression.Body.GetType().Name} ({GetType().Name}).", nameof(expression));

                var attribute = memberExpression.Member.GetSingleAttribute<XmlElementAttribute>() ?? memberExpression.Member.DeclaringType.GetElementAttributeFromInterface(memberExpression.Member as PropertyInfo);
                if (string.IsNullOrWhiteSpace(attribute?.ElementName))
                    throw new ArgumentException($"Specified member `{memberExpression.Member.Name}` does not define any XML element.", nameof(expression));

                headerDefinition.RequiredHeaders.Add(XName.Get(attribute.ElementName, attribute.Namespace));

                return this;
            }
        }

        private class XNameComparer : IComparer<XName>
        {
            public int Compare(XName x, XName y)
            {
                var ns = string.Compare(x.NamespaceName, y.NamespaceName);
                return ns != 0 ? ns : string.Compare(x.LocalName, y.LocalName);
            }
        }
    }
}
