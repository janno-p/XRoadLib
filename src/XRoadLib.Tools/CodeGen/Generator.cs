using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Tools.CodeGen.CodeFragments;
using XRoadLib.Tools.CodeGen.Extensions;

namespace XRoadLib.Tools.CodeGen
{
    public class Generator
    {
        private readonly XDocument document;
        private readonly DirectoryInfo directory;

        public Generator(XDocument document, DirectoryInfo directory)
        {
            this.document = document;
            this.directory = directory;
        }

        public void Generate()
        {
            var definitionsElement = document.Element(XName.Get("definitions", NamespaceConstants.WSDL));

            var serviceGenerator = new ServiceGenerator(definitionsElement.Element(XName.Get("service", NamespaceConstants.WSDL)));
            serviceGenerator.Generate().SaveFile(Path.Combine(directory.FullName, $"{serviceGenerator.ServiceName}.cs"));

            var referencedTypes = new Dictionary<XmlQualifiedName, bool>();

            definitionsElement
                .Elements(XName.Get("portType", NamespaceConstants.WSDL))
                .Select(e => new PortTypeGenerator(e))
                .ToList()
                .ForEach(g => g.Generate().SaveFile(Path.Combine(directory.FullName, $"{g.PortTypeName}.cs")));

            definitionsElement
                .Elements(XName.Get("binding", NamespaceConstants.WSDL))
                .Select(e => new BindingGenerator(e))
                .ToList()
                .ForEach(g => g.Generate().SaveFile(Path.Combine(directory.FullName, $"{g.BindingName}.cs")));

            definitionsElement
                .Elements(XName.Get("types", NamespaceConstants.WSDL))
                .SelectMany(e => e.Elements(XName.Get("schema", NamespaceConstants.XSD)))
                .ToList()
                .ForEach(x => GenerateSchema(x, referencedTypes));

            if (referencedTypes.Any(kvp => !kvp.Value))
                throw new Exception($"Types not defined: `{(string.Join(", ", referencedTypes.Where(x => !x.Value).Select(x => x.Key)))}`.");
        }

        private void GenerateSchema(XElement schemaElement, IDictionary<XmlQualifiedName, bool> referencedTypes)
        {
            var typesDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "Types"));
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
                   p.ParseElement("import") ||
                   p.ParseElement("redefine") ||
                   p.IgnoreElement("annotation"))
            { }

            while (p.ParseElement("simpleType") ||
                   p.ParseElement("complexType", x => ParseComplexType(x, targetNamespace, referencedTypes, typesDirectory)) ||
                   p.ParseElement("group") ||
                   p.ParseElement("attributeGroup") ||
                   p.ParseElement("element") ||
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
    }
}