using System;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XRoadLib.Tools.CodeGen.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen
{
    public class ComplexTypeGenerator
    {
        private readonly XElement element;

        public string TypeName { get; }

        public ComplexTypeGenerator(XElement element)
            : this(element, element.Attribute("name").Value)
        { }

        public ComplexTypeGenerator(XElement element, string typeName)
        {
            this.element = element;
            TypeName = typeName;
        }

        public CompilationUnitSyntax Generate()
        {
            return CompilationUnit()
                     .AddUsings(UsingDirective(IdentifierName("System")),
                                UsingDirective(IdentifierName("System.IO")),
                                UsingDirective(IdentifierName("System.Xml")),
                                UsingDirective(IdentifierName("XRoadLib.Serialization")))
                     .AddMembers(NamespaceDeclaration(IdentifierName("MyNamespace"))
                                   .AddMembers(GenerateType()));
        }

        public ClassDeclarationSyntax GenerateType()
        {
            var type = ClassDeclaration(TypeName).AddBaseListTypes(SimpleBaseType(ParseTypeName("IXRoadXmlSerializable")));

            var readXml = MethodDeclaration(ParseTypeName("void"), "ReadXml")
                            .AddParameterListParameters(Parameter(Identifier("reader")).WithType(ParseTypeName("XmlReader")),
                                                        Parameter(Identifier("message")).WithType(ParseTypeName("XRoadMessage")))
                            .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName("IXRoadXmlSerializable")))
                            .WithBody(Block());

            var writeXml = MethodDeclaration(ParseTypeName("void"), "WriteXml")
                             .AddParameterListParameters(Parameter(Identifier("writer")).WithType(ParseTypeName("XmlWriter")),
                                                         Parameter(Identifier("message")).WithType(ParseTypeName("XRoadMessage")))
                             .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName("IXRoadXmlSerializable")))
                             .WithBody(Block());

            var parseElement = new Action<XElement>(e =>
            {
                var propertyName = Identifier(e.Attribute("name").Value);
                var propertyType = GetPredefinedType(e);

                var p = new XElementParser(e);
                p.IgnoreElement("annotation");

                if (p.ParseElement("simpleType") ||
                    p.ParseElement("complexType", x =>
                    {
                        var propertyTypeName = $"{e.Attribute("name").Value}Type";
                        propertyType = ParseTypeName(propertyTypeName);
                        type = type.AddMembers(new ComplexTypeGenerator(x, propertyTypeName).GenerateType());
                    }))
                { }

                while (p.ParseElement("unique") ||
                       p.ParseElement("key") ||
                       p.ParseElement("keyref"))
                { }

                var prop = PropertyDeclaration(propertyType, propertyName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                              AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

                type = type.AddMembers(prop);
            });

            var parseSequence = new Action<XElement>(e =>
            {
                var p = new XElementParser(e);
                p.IgnoreElement("annotation");

                while (p.ParseElement("element", parseElement) ||
                       p.ParseElement("group") ||
                       p.ParseElement("choice") ||
                       p.ParseElement("sequence") ||
                       p.ParseElement("any"))
                { }

                p.ThrowIfNotDone();
            });

            var parser = new XElementParser(element);

            if (!parser.IgnoreElement("annotation"))
                type = type.AddModifiers(Token(SyntaxKind.PublicKeyword));

            if (!parser.ParseElement("simpleContent") && !parser.ParseElement("complexContent"))
            {
                if (parser.ParseElement("group") ||
                    parser.ParseElement("all") ||
                    parser.ParseElement("choice") ||
                    parser.ParseElement("sequence", parseSequence))
                { }

                while (parser.ParseElement("attribute") ||
                       parser.ParseElement("attributeGroup"))
                { }

                parser.ParseElement("anyAttribute");
            }

            parser.ThrowIfNotDone();

            return type.AddMembers(readXml, writeXml);
        }

        private TypeSyntax GetPredefinedType(XElement element)
        {
            var typeName = element.GetAttributeAsXName("type");

            if (typeName.NamespaceName == NamespaceConstants.XSD)
                switch (typeName.LocalName)
                {
                    case "base64Binary": return ParseTypeName("Stream");
                    case "date": return ParseTypeName("DateTime");
                    case "long": return PredefinedType(Token(SyntaxKind.LongKeyword));
                    case "string": return PredefinedType(Token(SyntaxKind.StringKeyword));
                }

            throw new NotImplementedException(typeName.ToString());
        }
    }
}