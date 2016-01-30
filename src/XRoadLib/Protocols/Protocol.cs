using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private IDictionary<uint, ISerializerCache> versioningSerializerCaches;
        private ISerializerCache serializerCache;

        public IEnumerable<uint> SupportedVersions => versioningSerializerCaches?.Keys ?? Enumerable.Empty<uint>();

        public abstract string Name { get; }

        public Assembly ContractAssembly { get; private set; }

        protected abstract string XRoadPrefix { get; }
        protected abstract string XRoadNamespace { get; }

        public virtual string RequestPartNameInRequest => "request";
        public virtual string RequestPartNameInResponse => "request";
        public virtual string ResponsePartNameInResponse => "response";

        public Style Style { get; }
        public string ProducerNamespace { get; }
        public ISet<XName> MandatoryHeaders { get; } = new SortedSet<XName>();
        public ISchemaExporter SchemaExporter { get; }

        protected Protocol(string producerNamespace, Style style, ISchemaExporter schemaExporter)
        {
            if (string.IsNullOrWhiteSpace(producerNamespace))
                throw new ArgumentNullException(nameof(producerNamespace));
            ProducerNamespace = producerNamespace;

            if (style == null)
                throw new ArgumentNullException(nameof(style));
            Style = style;

            SchemaExporter = schemaExporter ?? new SchemaExporter(producerNamespace);
        }

        protected abstract void DefineMandatoryHeaderElements();

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

        public void WriteSoapHeader(XmlWriter writer, IXRoadHeader header, IEnumerable<XElement> additionalHeaders = null)
        {
            writer.WriteStartElement("Header", NamespaceConstants.SOAP_ENV);

            if (header is THeader)
                WriteSoapHeader(writer, (THeader)header);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            writer.WriteEndElement();
        }

        protected abstract void WriteSoapHeader(XmlWriter writer, THeader header);

        public abstract bool IsHeaderNamespace(string ns);

        public virtual bool IsDefinedByEnvelope(XmlReader reader)
        {
            return false;
        }

        public void WriteServiceDescription(Stream outputStream, uint? version = null)
        {
            throw new NotImplementedException();
        }

        public virtual XmlElement CreateOperationVersionElement(OperationDefinition operationDefinition)
        {
            if (operationDefinition.Version == 0)
                return null;

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

        public void SetContractAssembly(Assembly assembly, params uint[] supportedVersions)
        {
            if (ContractAssembly != null)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) already has contract configured.");

            ContractAssembly = assembly;

            if (supportedVersions == null || supportedVersions.Length == 0)
            {
                serializerCache = new SerializerCache(this, assembly);
                return;
            }

            versioningSerializerCaches = new Dictionary<uint, ISerializerCache>();
            foreach (var dtoVersion in supportedVersions)
                versioningSerializerCaches.Add(dtoVersion, new SerializerCache(this, assembly, dtoVersion));
        }

        public ISerializerCache GetSerializerCache(uint? version = null)
        {
            if (!version.HasValue)
            {
                if (serializerCache != null)
                    return serializerCache;
                throw new ArgumentNullException(nameof(version), $"This protocol instance (message protocol version `{Name}`) supports multiple versions.");
            }

            if (versioningSerializerCaches == null)
                throw new ArgumentException($"This protocol instance (message protocol version `{Name}`) doest not support multiple versions.", nameof(version));

            ISerializerCache versioningSerializerCache;
            if (versioningSerializerCaches.TryGetValue(version.Value, out versioningSerializerCache))
                return versioningSerializerCache;

            throw new ArgumentException($"This protocol instance (message protocol version `{Name}`) does not support `v{version.Value}`.", nameof(version));
        }

        protected void WriteHeaderElement(XmlWriter writer, string name, string value)
        {
            if (!MandatoryHeaders.Contains(name) && string.IsNullOrWhiteSpace(value))
                return;

            writer.WriteStartElement(name, XRoadNamespace);
            writer.WriteValue(value.GetValueOrDefault(""));
            writer.WriteEndElement();
        }
    }
}