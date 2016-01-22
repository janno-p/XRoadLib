using System.Linq;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Extensions;

namespace XRoadLib.Description
{
    internal class XRoadSchemaBuilder
    {
        private readonly XmlDocument document = new XmlDocument();

        private readonly string xroadPrefix;
        private readonly string xroadNamespace;

        internal XRoadSchemaBuilder(XRoadProtocol protocol)
        {
            xroadPrefix = protocol.GetPrefix();
            xroadNamespace = protocol.GetNamespace();
        }

        internal XmlSchemaAnnotation CreateAnnotationFor(ICustomAttributeProvider source)
        {
            var nodes = source.GetXRoadTitles()
                              .Where(title => !string.IsNullOrWhiteSpace(title.Item2))
                              .Select(title => CreateXRoadTitle(title.Item1, title.Item2))
                              .Cast<XmlNode>()
                              .ToArray();

            return nodes.Length > 0 ? new XmlSchemaAnnotation { Items = { new XmlSchemaAppInfo { Markup = nodes } } } : null;
        }

        internal XmlElement CreateDocumentationFor(ICustomAttributeProvider provider)
        {
            var nodes = provider.GetXRoadTitles()
                                .Where(title => !string.IsNullOrWhiteSpace(title.Item2))
                                .Select(title => CreateXRoadTitle(title.Item1, title.Item2))
                                .ToArray();

            if (nodes.Length < 1)
                return null;

            var documentationElement = document.CreateElement("wsdl", "documentation", NamespaceConstants.WSDL);

            foreach (var node in nodes)
                documentationElement.AppendChild(node);

            return documentationElement;
        }

        internal XmlElement CreateXRoadTitle(string languageCode, string value)
        {
            var titleElement = document.CreateElement(xroadPrefix, "title", xroadNamespace);
            titleElement.InnerText = value;

            if (string.IsNullOrWhiteSpace(languageCode))
                return titleElement;

            var attribute = document.CreateAttribute("xml", "lang", null);
            attribute.Value = languageCode;
            titleElement.Attributes.Append(attribute);

            return titleElement;
        }

        internal XmlElement CreateXRoadVersionBinding(uint value)
        {
            var addressElement = document.CreateElement(xroadPrefix, "version", xroadNamespace);
            addressElement.InnerText = $"v{value}";
            return addressElement;
        }

        internal XmlElement CreateSoapHeader(SoapHeaderBinding binding)
        {
            var element = document.CreateElement(PrefixConstants.SOAP, "header", NamespaceConstants.SOAP);

            element.SetAttribute("message", binding.Message.Name);
            element.SetAttribute("part", binding.Part);
            element.SetAttribute("use", binding.Use == SoapBindingUse.Encoded ? "encoded" : "literal");

            if (binding.Namespace != null)
                element.SetAttribute("namespace", binding.Namespace);

            if (binding.Encoding != null)
                element.SetAttribute("encodingStyle", binding.Encoding);

            return element;
        }

        internal XmlElement CreateAddressBinding(string producerName)
        {
            var element = document.CreateElement(xroadPrefix, "address", xroadNamespace);
            element.SetAttribute("producer", producerName);
            return element;
        }

        internal XmlAttribute CreateEncodedArrayTypeAttribute(XName qualifiedName)
        {
            var attribute = document.CreateAttribute(PrefixConstants.WSDL, "arrayType", NamespaceConstants.WSDL);
            attribute.Value = $"{qualifiedName.NamespaceName}:{qualifiedName.LocalName}[]";
            return attribute;
        }

        internal XmlAttribute CreateExpectedContentType(string contentType)
        {
            var attribute = document.CreateAttribute(PrefixConstants.XMIME, "expectedContentTypes", NamespaceConstants.XMIME);
            attribute.Value = contentType;
            return attribute;
        }
    }
}