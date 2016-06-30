#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Linq;
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

            WriteAttribute("name", serviceDescription.Name);
            WriteAttribute("targetNamespace", serviceDescription.TargetNamespace);

            WriteExtensions();
            WriteDocumentation();
            WriteImports();
            WriteTypes();
            WriteMessages();
            WritePortTypes();
            WriteBindings();
            serviceDescription.Services.ToList().ForEach(WriteService);

            writer.WriteEndElement();
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

        private void WriteDocumentation()
        {

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

        private void WritePort()
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
            WriteDocumentation();
            WritePort();

            writer.WriteEndElement();
        }

        private void WriteTypes()
        {

        }
    }
}

#endif