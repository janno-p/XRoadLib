using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XRoadLib.Tools.CodeGen.CodeFragments;
using XRoadLib.Tools.CodeGen.Extensions;

namespace XRoadLib.Tools.CodeGen
{
    public interface IGenerator
    {
        Task GenerateAsync();
    }

    public class Generator : IGenerator
    {
        private readonly ILogger logger;
        private readonly IOptions<GeneratorOptions> optionsAccessor;

        private GeneratorOptions Options => optionsAccessor.Value;

        public Generator(ILoggerFactory loggerFactory, IOptions<GeneratorOptions> optionsAccessor)
        {
            this.logger = loggerFactory.CreateLogger<Generator>();
            this.optionsAccessor = optionsAccessor;
        }

        public async Task GenerateAsync()
        {
            var document = await LoadServiceDescriptionAsync(Options.WsdlUri);

            if (!Options.OutputPath.Exists)
                Options.OutputPath.Create();

            var definitionsElement = document.Element(XName.Get("definitions", NamespaceConstants.WSDL));

            var serviceGenerator = new ServiceGenerator(definitionsElement.Element(XName.Get("service", NamespaceConstants.WSDL)));
            serviceGenerator.Generate().SaveFile(Path.Combine(Options.OutputPath.FullName, $"{serviceGenerator.ServiceName}.cs"));

            var referencedTypes = new Dictionary<XmlQualifiedName, bool>();

            definitionsElement
                .Elements(XName.Get("portType", NamespaceConstants.WSDL))
                .Select(e => new PortTypeGenerator(e))
                .ToList()
                .ForEach(g => g.Generate().SaveFile(Path.Combine(Options.OutputPath.FullName, $"{g.PortTypeName}.cs")));

            definitionsElement
                .Elements(XName.Get("binding", NamespaceConstants.WSDL))
                .Select(e => new BindingGenerator(e))
                .ToList()
                .ForEach(g => g.Generate().SaveFile(Path.Combine(Options.OutputPath.FullName, $"{g.BindingName}.cs")));

            definitionsElement
                .Elements(XName.Get("types", NamespaceConstants.WSDL))
                .SelectMany(e => e.Elements(XName.Get("schema", NamespaceConstants.XSD)))
                .ToList()
                .ForEach(x => ParseSchema(x, referencedTypes));

            if (referencedTypes.Any(kvp => !kvp.Value))
                throw new Exception($"Types not defined: `{(string.Join(", ", referencedTypes.Where(x => !x.Value).Select(x => x.Key)))}`.");
        }

        private async Task<XDocument> LoadServiceDescriptionAsync(string uri)
        {
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                logger.LogInformation($"Requesting service description from network location: {uri}.");
                using (var client = new HttpClient())
                using (var stream = await client.GetStreamAsync(uri))
                    return XDocument.Load(stream);
            }

            var fileInfo = new FileInfo(uri);
            if (fileInfo.Exists)
            {
                logger.LogInformation($"Requesting service description from file: {fileInfo.FullName}.");
                return XDocument.Load(fileInfo.FullName);
            }

            throw new FileNotFoundException($"Cannot resolve wsdl location `{uri}`.");
        }

        private void ParseSchema(XElement schemaElement, IDictionary<XmlQualifiedName, bool> referencedTypes)
        {
            var typesDirectory = new DirectoryInfo(Path.Combine(Options.OutputPath.FullName, "Types"));
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