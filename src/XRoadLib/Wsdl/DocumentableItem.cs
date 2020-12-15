using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public abstract class DocumentableItem
    {
        protected abstract string ElementName { get; }

        public XmlElement DocumentationElement { get; set; }
        public List<XmlAttribute> ExtensibleAttributes { get; } = new();
        public List<ServiceDescriptionFormatExtension> Extensions { get; } = new();
        public Dictionary<string, string> Namespaces { get; } = new();

        public string Documentation
        {
            get => DocumentationElement != null ? DocumentationElement.InnerText : "";
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    DocumentationElement = null;
                    return;
                }

                var doc = new XmlDocument();
                DocumentationElement = doc.CreateElement(PrefixConstants.Wsdl, "documentation", NamespaceConstants.Wsdl);
                DocumentationElement.InnerText = value;
            }
        }

        internal async Task WriteAsync(XmlWriter writer)
        {
            await WriteStartElementAsync(writer, ElementName).ConfigureAwait(false);
            await WriteAttributesAsync(writer).ConfigureAwait(false);
            await WriteElementsAsync(writer).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        protected virtual async Task WriteAttributesAsync(XmlWriter writer)
        {
            var namespaces =
                Namespaces.Where(x => !string.IsNullOrWhiteSpace(x.Value) && writer.LookupPrefix(x.Value) != x.Key)
                          .ToList();

            foreach (var ns in namespaces)
                await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, ns.Key, NamespaceConstants.Xmlns, ns.Value).ConfigureAwait(false);

            ExtensibleAttributes.ForEach(x => x.WriteTo(writer));
        }

        protected virtual async Task WriteElementsAsync(XmlWriter writer)
        {
            foreach (var extension in Extensions)
                await extension.WriteAsync(writer).ConfigureAwait(false);

            DocumentationElement?.WriteTo(writer);
        }

        protected Task WriteStartElementAsync(XmlWriter writer, string name)
        {
            var prefix =
                Namespaces.Where(kvp => kvp.Value == NamespaceConstants.Wsdl)
                          .Select(kvp => kvp.Key)
                          .SingleOrDefault()
                ?? writer.LookupPrefix(NamespaceConstants.Wsdl)
                ?? "";

            return writer.WriteStartElementAsync(prefix, name, NamespaceConstants.Wsdl);
        }
    }
}