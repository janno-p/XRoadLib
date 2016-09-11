using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using XRoadLib.Tools.CodeGen.CodeFragments;
using XRoadLib.Tools.CodeGen.Extensions;

namespace XRoadLib.Tools.CodeGen
{
    public interface IGenerator
    {
        void Generate(XDocument document, DirectoryInfo directoryInfo);
    }

    public class Generator : IGenerator
    {
        private readonly ILogger logger;

        public Generator(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<Generator>();
        }

        public void Generate(XDocument document, DirectoryInfo directoryInfo)
        {
            var definitionsElement = document.Element(XName.Get("definitions", NamespaceConstants.WSDL));

            var serviceGenerator = new ServiceGenerator(definitionsElement.Element(XName.Get("service", NamespaceConstants.WSDL)));
            serviceGenerator.Generate().SaveFile(Path.Combine(directoryInfo.FullName, $"{serviceGenerator.ServiceName}.cs"));

            var referencedTypes = new Dictionary<XmlQualifiedName, bool>();

            definitionsElement
                .Elements(XName.Get("portType", NamespaceConstants.WSDL))
                .Select(e => new PortTypeGenerator(e))
                .ToList()
                .ForEach(g => g.Generate().SaveFile(Path.Combine(directoryInfo.FullName, $"{g.PortTypeName}.cs")));

            definitionsElement
                .Elements(XName.Get("binding", NamespaceConstants.WSDL))
                .Select(e => new BindingGenerator(e))
                .ToList()
                .ForEach(g => g.Generate().SaveFile(Path.Combine(directoryInfo.FullName, $"{g.BindingName}.cs")));

            definitionsElement
                .Elements(XName.Get("types", NamespaceConstants.WSDL))
                .SelectMany(e => e.Elements(XName.Get("schema", NamespaceConstants.XSD)))
                .ToList()
                .ForEach(x => ParseSchema(x, referencedTypes, directoryInfo));

            if (referencedTypes.Any(kvp => !kvp.Value))
                throw new Exception($"Types not defined: `{(string.Join(", ", referencedTypes.Where(x => !x.Value).Select(x => x.Key)))}`.");
        }

        private void ParseSchema(XElement schemaElement, IDictionary<XmlQualifiedName, bool> referencedTypes, DirectoryInfo directoryInfo)
        {
            var typesDirectory = new DirectoryInfo(Path.Combine(directoryInfo.FullName, "Types"));
            if (!typesDirectory.Exists)
                typesDirectory.Create();

            var p = new XElementParser(schemaElement);

            p.AttributeNotImplemented("attributeFormDefault");
            p.AttributeNotImplemented("elementFormDefault");
            p.AttributeNotImplemented("blockDefault");
            p.AttributeNotImplemented("finalDefault");
            p.AttributeNotImplemented("version");
            p.AttributeNotImplemented("xmlns");

            var targetNamespace = schemaElement.Attribute("targetNamespace").Value;

            while (p.ParseElement("include") ||
                   p.ParseElement("import", x => ParseImport(x)) ||
                   p.ParseElement("redefine") ||
                   p.IgnoreElement("annotation"))
            { }

            while (p.ParseElement("simpleType") ||
                   p.ParseElement("complexType", x => ParseComplexType(x, targetNamespace, referencedTypes, typesDirectory)) ||
                   p.ParseElement("group") ||
                   p.ParseElement("attributeGroup") ||
                   p.ParseElement("element", x => ParseElement(x)) ||
                   p.ParseElement("attribute") ||
                   p.ParseElement("notation") ||
                   p.ParseElement("annotation"))
            { }

            p.ThrowIfNotDone();
        }

        private void ParseComplexType(XElement element, string targetNamespace, IDictionary<XmlQualifiedName, bool> referencedTypes, DirectoryInfo typesDirectory)
        {
            new ComplexTypeFragment(targetNamespace, element.GetName(), element)
                .BuildTypeDeclaration(referencedTypes)
                .SaveToFile(typesDirectory.FullName);
        }

        private void ParseElement(XElement element)
        {
            logger.LogWarning("Skipping `element` element.");
        }

        private void ParseImport(XElement element)
        {
            logger.LogWarning("Skipping `import` element.");
            // return;

            // var p = new XElementParser(element);

            // //p.AttributeNotImplemented("namespace");
            // if (element.HasAttribute("namespace"))
            //     logger.LogWarning("Not implemented `namespace` attribute.");

            // //p.AttributeNotImplemented("schemaLocation");
            // if (element.HasAttribute("schemaLocation"))
            //     logger.LogWarning("Not implemented `schemaLocation` attribute.");

            // p.IgnoreElement("annotation");
        }
    }
}