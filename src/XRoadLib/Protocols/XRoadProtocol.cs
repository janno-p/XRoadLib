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
using XRoadLib.Protocols.Description;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Protocols
{
    public abstract class XRoadProtocol
    {
        protected readonly XmlDocument document = new XmlDocument();

        private readonly SchemaDefinitionReader schemaDefinitionReader;

        private IDictionary<uint, ISerializerCache> versioningSerializerCaches;
        private ISerializerCache serializerCache;

        protected abstract string XRoadPrefix { get; }
        protected abstract string XRoadNamespace { get; }

        internal ISet<XName> MandatoryHeaders { get; } = new SortedSet<XName>(new XNameComparer());

        public abstract string Name { get; }

        public virtual string RequestPartNameInRequest => "request";
        public virtual string RequestPartNameInResponse => "request";
        public virtual string ResponsePartNameInResponse => "response";

        public IEnumerable<uint> SupportedVersions => versioningSerializerCaches?.Keys ?? Enumerable.Empty<uint>();

        public Style Style { get; }
        public string ProducerNamespace { get; }
        public Assembly ContractAssembly { get; private set; }

        protected abstract void DefineMandatoryHeaderElements();

        protected XRoadProtocol(string producerNamespace, Style style, ISchemaExporter schemaExporter)
        {
            if (style == null)
                throw new ArgumentNullException(nameof(style));
            Style = style;

            if (string.IsNullOrWhiteSpace(producerNamespace))
                throw new ArgumentNullException(nameof(producerNamespace));
            ProducerNamespace = producerNamespace;

            schemaDefinitionReader = new SchemaDefinitionReader(producerNamespace, schemaExporter);
        }

        internal abstract bool IsHeaderNamespace(string ns);

        internal virtual bool IsDefinedByEnvelope(XmlReader reader)
        {
            return false;
        }

        public virtual void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            serviceDescription.Namespaces.Add(XRoadPrefix, XRoadNamespace);
        }

        internal abstract IXRoadHeader CreateHeader();

        internal virtual void WriteSoapEnvelope(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.SOAP_ENV, "Envelope", NamespaceConstants.SOAP_ENV);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.SOAP_ENV, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENV);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.XSD, NamespaceConstants.XMLNS, NamespaceConstants.XSD);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.XSI, NamespaceConstants.XMLNS, NamespaceConstants.XSI);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.TARGET, NamespaceConstants.XMLNS, ProducerNamespace);
        }

        protected abstract void WriteXRoadHeader(XmlWriter writer, IXRoadHeader header);

        public void WriteSoapHeader(XmlWriter writer, IXRoadHeader header, IEnumerable<XElement> additionalHeaders = null)
        {
            writer.WriteStartElement("Header", NamespaceConstants.SOAP_ENV);

            WriteXRoadHeader(writer, header);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            writer.WriteEndElement();
        }

        public void WriteServiceDescription(Stream outputStream, uint? version = null)
        {
            new ProducerDefinition(this, schemaDefinitionReader, ContractAssembly, version).SaveTo(outputStream);
        }

        internal XmlElement CreateOperationVersionElement(OperationDefinition operationDefinition)
        {
            if (operationDefinition.Version == 0)
                return null;

            var addressElement = document.CreateElement(XRoadPrefix, "version", XRoadNamespace);
            addressElement.InnerText = $"v{operationDefinition.Version}";
            return addressElement;
        }

        internal XmlElement CreateTitleElement(string languageCode, string value)
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
            DefineMandatoryHeaderElements();

            if (supportedVersions == null || supportedVersions.Length == 0)
            {
                serializerCache = new SerializerCache(this, schemaDefinitionReader, assembly);
                return;
            }

            versioningSerializerCaches = new Dictionary<uint, ISerializerCache>();
            foreach (var dtoVersion in supportedVersions)
                versioningSerializerCaches.Add(dtoVersion, new SerializerCache(this, schemaDefinitionReader, assembly, dtoVersion));
        }

        public ISerializerCache GetSerializerCache(uint? version = null)
        {
            if (serializerCache == null && versioningSerializerCaches == null)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) is not configured with contract assembly.");

            if (serializerCache != null)
                return serializerCache;

            if (!version.HasValue)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) requires specific version value.");

            ISerializerCache versioningSerializerCache;
            if (versioningSerializerCaches.TryGetValue(version.Value, out versioningSerializerCache))
                return versioningSerializerCache;

            throw new ArgumentException($"This protocol instance (message protocol version `{Name}`) does not support `v{version.Value}`.", nameof(version));
        }

        protected void AddMandatoryHeaderElement<THeader, T>(Expression<Func<THeader, T>> expression) where THeader : IXRoadHeader
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Only MemberExpression is allowed to use for SOAP header definition, but was {expression.Body.GetType().Name} ({GetType().Name}).", nameof(expression));

            var attribute = memberExpression.Member.GetSingleAttribute<XmlElementAttribute>() ?? GetElementAttributeFromInterface(memberExpression.Member.DeclaringType, memberExpression.Member as PropertyInfo);
            if (string.IsNullOrWhiteSpace(attribute?.ElementName))
                throw new ArgumentException($"Specified member `{memberExpression.Member.Name}` does not define any XML element.", nameof(expression));

            MandatoryHeaders.Add(XName.Get(attribute.ElementName, attribute.Namespace));
        }

        private static XmlElementAttribute GetElementAttributeFromInterface(Type declaringType, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            var getMethod = propertyInfo.GetGetMethod();

            foreach (var iface in declaringType.GetInterfaces())
            {
                var map = declaringType.GetInterfaceMap(iface);

                var index = -1;
                for (var i = 0; i < map.TargetMethods.Length; i++)
                    if (map.TargetMethods[i] == getMethod)
                    {
                        index = i;
                        break;
                    }

                if (index < 0)
                    continue;

                var ifaceProperty = iface.GetProperties().SingleOrDefault(p => p.GetGetMethod() == map.InterfaceMethods[index]);

                var attribute = ifaceProperty.GetSingleAttribute<XmlElementAttribute>();
                if (attribute != null)
                    return attribute;
            }

            return null;
        }

        protected void WriteHeaderElement(XmlWriter writer, string name, object value)
        {
            if (!MandatoryHeaders.Contains(name) && value == null)
                return;

            writer.WriteStartElement(name, XRoadNamespace);

            var stringValue = value as string;
            if (stringValue != null)
                writer.WriteCDataEscape(stringValue);
            else writer.WriteValue(value);

            writer.WriteEndElement();
        }

        private class XNameComparer : IComparer<XName>
        {
            public int Compare(XName x, XName y)
            {
                var ns = string.Compare(x.NamespaceName, y.NamespaceName, StringComparison.InvariantCulture);
                return ns != 0 ? ns : string.Compare(x.LocalName, y.LocalName, StringComparison.InvariantCulture);
            }
        }
    }
}