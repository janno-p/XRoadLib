using System;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XRoadLib.Tools.CodeGen.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class ComplexTypeFragment : ICodeFragment
    {
        private readonly string elementName;
        private readonly bool isOptional;
        private readonly TypeSyntax typeSyntax;
        private readonly XElement definition;
        private readonly string typeName;

        public ComplexTypeFragment(string typeName, XElement definition)
            : this(null, typeName, false, definition)
        { }

        public ComplexTypeFragment(string elementName, string typeName, bool isOptional, XElement definition)
        {
            this.elementName = elementName;
            this.isOptional = isOptional;
            this.typeName = typeName;
            this.typeSyntax = ParseTypeName(typeName);
            this.definition = definition;
        }

        public SyntaxList<StatementSyntax> BuildDeserializationStatements()
        {
            throw new NotImplementedException();
        }

        public PropertyDeclarationSyntax BuildPropertyDeclaration()
        {
            return SyntaxHelper.BuildProperty(typeSyntax, isOptional, Identifier(elementName));
        }

        public SyntaxList<StatementSyntax> BuildSerializationStatements()
        {
            throw new NotImplementedException();
        }

        public ClassDeclarationSyntax BuildTypeDeclaration()
        {
            var type = ClassDeclaration(typeName).AddBaseListTypes(SimpleBaseType(ParseTypeName("IXRoadXmlSerializable")));

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

            var parser = new XElementParser(definition);

            if (!parser.IgnoreElement("annotation"))
                type = type.AddModifiers(Token(SyntaxKind.PublicKeyword));

            if (!parser.ParseElement("simpleContent") && !parser.ParseElement("complexContent"))
            {
                if (parser.ParseElement("group") ||
                    parser.ParseElement("all") ||
                    parser.ParseElement("choice") ||
                    parser.ParseElement("sequence", x => ParseSequence(x, ref type)))
                { }

                while (parser.ParseElement("attribute") ||
                       parser.ParseElement("attributeGroup"))
                { }

                parser.ParseElement("anyAttribute");
            }

            parser.ThrowIfNotDone();

            return type.AddMembers(readXml, writeXml);
        }

        private void ParseSequence(XElement element, ref ClassDeclarationSyntax type)
        {
            var modifiedType = type;

            var p = new XElementParser(element);
            p.IgnoreElement("annotation");

            while (p.ParseElement("element", x => ParseElement(x, ref modifiedType)) ||
                    p.ParseElement("group") ||
                    p.ParseElement("choice") ||
                    p.ParseElement("sequence") ||
                    p.ParseElement("any"))
            { }

            p.ThrowIfNotDone();

            type = modifiedType;
        }

        private void ParseElement(XElement element, ref ClassDeclarationSyntax type)
        {
            var modifiedType = type;
            var fragment = CodeFragmentFactory.GetElementFragment(element);

            var p = new XElementParser(element);
            p.IgnoreElement("annotation");

            if (p.ParseElement("simpleType") ||
                p.ParseElement("complexType", x =>
                {
                    var complexTypeName = $"{element.Attribute("name").Value}Type";
                    fragment = new ComplexTypeFragment(element.Attribute("name").Value, complexTypeName, element.IsOptional(), x);

                    var newType = fragment.BuildTypeDeclaration();
                    if (newType != null)
                        modifiedType = modifiedType.AddMembers(newType);
                }))
            { }

            while (p.ParseElement("unique") ||
                    p.ParseElement("key") ||
                    p.ParseElement("keyref"))
            { }

            if (fragment != null)
                modifiedType = modifiedType.AddMembers(fragment.BuildPropertyDeclaration());

            type = modifiedType;
        }
    }
}