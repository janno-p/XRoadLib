using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Protocols.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Protocols
{
    public abstract class Protocol<THeader> : IProtocol<THeader> where THeader : IXRoadHeader, new()
    {
        public abstract string Name { get; }

        public Style Style { get; }
        public string ProducerNamespace { get; }
        public ISerializerCache SerializerCache { get; }
        public ISet<XName> MandatoryHeaders { get; } = new SortedSet<XName>();

        protected Protocol(string producerNamespace, Style style)
        {
            if (string.IsNullOrWhiteSpace(producerNamespace))
                throw new ArgumentNullException(nameof(producerNamespace));
            ProducerNamespace = producerNamespace;

            if (style == null)
                throw new ArgumentNullException(nameof(style));
            Style = style;
        }

        protected abstract void DefineMandatoryHeaderElements();

        public virtual void ExportType(TypeDefinition type)
        { }

        public virtual void ExportOperation(OperationDefinition operation)
        { }

        public virtual void ExportServiceDescription(ServiceDescription serviceDescription, Context context)
        { }

        public void AddMandatoryHeaderElement<T>(Expression<Func<THeader, T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Only MemberExpression is allowed to use for SOAP header definition, but was {expression.Body.GetType().Name} ({GetType().Name}).", nameof(expression));

            var attribute = memberExpression.Member.GetSingleAttribute<XmlElementAttribute>();
            if (string.IsNullOrWhiteSpace(attribute?.ElementName))
                throw new ArgumentException($"Specified member `{memberExpression.Member.Name}` does not define any XML element.", nameof(expression));

            MandatoryHeaders.Add(XName.Get(attribute.ElementName, attribute.Namespace));
        }

        public virtual IXRoadHeader CreateHeader()
        {
            return new THeader();
        }

        public abstract bool IsHeaderNamespace(string ns);

        public virtual bool IsDefinedByEnvelope(XmlReader reader)
        {
            return false;
        }
    }
}