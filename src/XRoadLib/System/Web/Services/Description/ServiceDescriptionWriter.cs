#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;
using XRoadLib;

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
            writer.WriteStartElement("definitions", NamespaceConstants.WSDL);

            WriteNamespaceDeclarations(serviceDescription.Namespaces);

            serviceDescription.ExtensibleAttributes.ForEach(WriteAttribute);

            WriteAttribute("name", serviceDescription.Name);
            WriteAttribute("targetNamespace", serviceDescription.TargetNamespace);

            WriteExtensions();
            WriteDocumentation(serviceDescription.DocumentationElement);
            WriteImports();
            WriteTypes();
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

        private void WriteExtensions()
        {

        }

        private void WriteImports()
        {

        }

        private void WriteMessages()
        {

        }

        private void WriteNamespaceDeclarations(IDictionary<string, string> namespaces)
        {

        }

        private void WritePort(Port port)
        {

        }

        private void WritePortTypes()
        {

        }

        private void WriteService(Service service)
        {
            writer.WriteStartElement("service", NamespaceConstants.WSDL);

            WriteNamespaceDeclarations(service.Namespaces);

            WriteAttribute("name", service.Name);

            WriteExtensions();
            WriteDocumentation(service.DocumentationElement);
            service.Ports.ForEach(WritePort);

            writer.WriteEndElement();
        }

        private void WriteTypes()
        {

        }
    }
}

#endif