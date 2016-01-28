using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Protocols
{
    public abstract class Protocol<THeader> : IProtocol<THeader> where THeader : IXRoadHeader, new()
    {
        protected readonly XmlDocument document = new XmlDocument();

        public abstract string Name { get; }

        protected abstract string XRoadPrefix { get; }
        protected abstract string XRoadNamespace { get; }

        public virtual string RequestPartNameInRequest => "request";
        public virtual string RequestPartNameInResponse => "request";
        public virtual string ResponsePartNameInResponse => "response";

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

        public virtual void ExportParameter(ParameterDefinition parameter)
        { }

        public virtual void ExportProperty(PropertyDefinition parameter)
        { }

        public virtual void ExportType(TypeDefinition type)
        {
            if (type.RuntimeInfo.IsArray)
                type.Name = null;
        }

        public virtual void ExportOperation(OperationDefinition operation)
        { }

        public virtual void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            serviceDescription.Namespaces.Add(XRoadPrefix, XRoadNamespace);
        }

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

        public void WriteServiceDescription(Assembly contractAssembly, Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public virtual XmlElement CreateOperationVersionElement(OperationDefinition operationDefinition)
        {
            if (operationDefinition.Version == 0)
                return null;

            var document = new XmlDocument();

            var addressElement = document.CreateElement(XRoadPrefix, "version", XRoadNamespace);
            addressElement.InnerText = $"v{operationDefinition.Version}";
            return addressElement;
        }

        public virtual XmlElement CreateTitleElement(string languageCode, string value)
        {
            var titleElement = document.CreateElement(XRoadPrefix, "title", XRoadNamespace);
            titleElement.InnerText = value;

            if (string.IsNullOrWhiteSpace(languageCode))
                return titleElement;

            var attribute = document.CreateAttribute("xml", "lang", null);
            attribute.Value = languageCode;
            titleElement.Attributes.Append(attribute);

            return titleElement;
        }
    }
}