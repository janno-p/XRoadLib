#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib;
using XRoadLib.Xml.Schema;

namespace System.Web.Services.Description
{
    internal class ServiceDescriptionWriter
    {
        private readonly XmlWriter writer;

        public ServiceDescriptionWriter(XmlWriter writer)
        {
            this.writer = writer;
        }

        public void WriteServiceDescription(ServiceDescription serviceDescription)
        {
            WriteStartElement("definitions", NamespaceConstants.WSDL, serviceDescription);

            WriteNamespaceDeclarations(serviceDescription.Namespaces);

            serviceDescription.ExtensibleAttributes.ForEach(WriteAttribute);

            WriteAttribute("name", serviceDescription.Name);
            WriteAttribute("targetNamespace", serviceDescription.TargetNamespace);

            serviceDescription.Extensions.ForEach(WriteExtension);
            WriteDocumentation(serviceDescription.DocumentationElement);
            //serviceDescription.Imports.ForEach(WriteImports);
            WriteTypes(serviceDescription.Types);
            WriteMessages();
            WritePortTypes();
            WriteBindings();
            serviceDescription.Services.ForEach(WriteService);

            writer.WriteEndElement();
        }

        private void WriteAttribute(XmlAttribute attribute)
        {
            if (attribute != null)
                writer.WriteAttributeString(attribute.LocalName, attribute.NamespaceURI, attribute.Value);
        }

        private void WriteAttribute(string name, string value)
        {
            WriteAttribute(name, "", value);
        }

        private void WriteAttribute(string name, string ns, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            writer.WriteAttributeString(name, ns, value);
        }

        private void WriteQualifiedAttribute(string name, XmlQualifiedName qualifiedName)
        {
            if (qualifiedName.IsEmpty)
                return;

            writer.WriteStartAttribute(name);
            writer.WriteQualifiedName(qualifiedName.Name, qualifiedName.Namespace);
            writer.WriteEndAttribute();
        }

        private void WriteBindings()
        {

        }

        private void WriteDocumentation(XmlElement documentationElement)
        {
            if (documentationElement == null)
                return;

            writer.WriteStartElement(documentationElement.LocalName, documentationElement.NamespaceURI);
            documentationElement.WriteTo(writer);
            writer.WriteEndElement();
        }

        private void WriteExtension(ServiceDescriptionFormatExtension extension)
        {
            extension.Write(writer);
        }

        private void WriteMessages()
        {

        }

        private void WriteNamespaceDeclarations(IDictionary<string, string> namespaces)
        {
            namespaces.Where(x => !string.IsNullOrWhiteSpace(x.Value) && writer.LookupPrefix(x.Value) != x.Key)
                      .ToList()
                      .ForEach(ns => writer.WriteAttributeString("xmlns", ns.Key, NamespaceConstants.XMLNS, ns.Value));
        }

        private void WritePort(Port port)
        {
            WriteStartElement("port", NamespaceConstants.WSDL, port);

            WriteNamespaceDeclarations(port.Namespaces);

            port.ExtensibleAttributes.ForEach(WriteAttribute);

            WriteAttribute("name", port.Name);
            WriteQualifiedAttribute("binding", port.Binding);

            port.Extensions.ForEach(WriteExtension);

            WriteDocumentation(port.DocumentationElement);

            writer.WriteEndElement();
        }

        private void WritePortTypes()
        {

        }

        private void WriteSchema(XmlSchema schema)
        {

        }

        private void WriteService(Service service)
        {
            WriteStartElement("service", NamespaceConstants.WSDL, service);

            WriteNamespaceDeclarations(service.Namespaces);

            WriteAttribute("name", service.Name);

            service.Extensions.ForEach(WriteExtension);
            WriteDocumentation(service.DocumentationElement);
            service.Ports.ForEach(WritePort);

            writer.WriteEndElement();
        }

        private void WriteStartElement(string name, string ns, DocumentableItem documentableItem)
        {
            var prefix = documentableItem.Namespaces
                                         .Where(kvp => kvp.Value == NamespaceConstants.WSDL)
                                         .Select(kvp => kvp.Key).SingleOrDefault() ?? writer.LookupPrefix(ns);
            writer.WriteStartElement(prefix, name, ns);
        }

        private void WriteTypes(Types types)
        {
            WriteStartElement("types", NamespaceConstants.WSDL, types);

            WriteNamespaceDeclarations(types.Namespaces);

            types.ExtensibleAttributes.ForEach(WriteAttribute);
            types.Extensions.ForEach(WriteExtension);

            WriteDocumentation(types.DocumentationElement);

            types.Schemas.ForEach(WriteSchema);

            writer.WriteEndElement();
        }
    }
}

#endif