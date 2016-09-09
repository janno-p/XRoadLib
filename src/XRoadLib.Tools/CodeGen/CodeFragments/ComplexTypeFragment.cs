using System;
using System.Collections.Generic;
using System.Xml;
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

        public IPropertyFragment PropertyFragment { get; }

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

            PropertyFragment = new PropertyFragment(typeSyntax, elementName, false, isOptional);
        }

        public SyntaxList<StatementSyntax> BuildDeserializationStatements()
        {
            throw new NotImplementedException();
        }

        public SyntaxList<StatementSyntax> BuildSerializationStatements()
        {
            throw new NotImplementedException();
        }

        public ClassDeclarationSyntax BuildTypeDeclaration(IDictionary<XmlQualifiedName, bool> referencedTypes)
        {
            var type = ClassDeclaration(typeName);

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

            if (!parser.ParseElement("simpleContent")
                && !parser.ParseElement("complexContent", x => ParseComplexContent(x, referencedTypes, ref type)))
            {
                if (parser.ParseElement("group") ||
                    parser.ParseElement("all") ||
                    parser.ParseElement("choice") ||
                    parser.ParseElement("sequence", x => ParseSequence(x, referencedTypes, ref type)))
                { }

                while (parser.ParseElement("attribute") ||
                       parser.ParseElement("attributeGroup"))
                { }

                parser.ParseElement("anyAttribute");
            }

            parser.ThrowIfNotDone();

            if (type.BaseList == null || !type.BaseList.Types.Any())
                type = type.AddBaseListTypes(SimpleBaseType(ParseTypeName("IXRoadXmlSerializable")));

            return type.AddMembers(readXml, writeXml);
        }

        private void ParseSequence(XElement element, IDictionary<XmlQualifiedName, bool> referencedTypes, ref ClassDeclarationSyntax type)
        {
            var modifiedType = type;

            var p = new XElementParser(element);
            p.IgnoreElement("annotation");

            while (p.ParseElement("element", x => ParseElement(x, referencedTypes, ref modifiedType)) ||
                   p.ParseElement("group") ||
                   p.ParseElement("choice") ||
                   p.ParseElement("sequence") ||
                   p.ParseElement("any"))
            { }

            p.ThrowIfNotDone();

            type = modifiedType;
        }

        private void ParseElement(XElement element, IDictionary<XmlQualifiedName, bool> referencedTypes, ref ClassDeclarationSyntax type)
        {
            var modifiedType = type;
            var fragment = CodeFragmentFactory.GetElementFragment(element, referencedTypes);

            if (element.GetMaxOccurs() > 1)
                throw new NotImplementedException("Collection of elements");

            if ((element.Attribute("abstract")?.AsBoolean()).GetValueOrDefault(false))
                modifiedType = modifiedType.AddModifiers(Token(SyntaxKind.AbstractKeyword));

            if (element.HasAttribute("block") ||
                element.HasAttribute("final") ||
                element.HasAttribute("form") ||
                element.HasAttribute("fixed") ||
                element.HasAttribute("default") ||
                element.HasAttribute("substitutionGroup") ||
                element.HasAttribute("ref"))
            {
                throw new NotImplementedException("Element attributes `block, final, form, fixed, default, substitutionGroup, ref` are not implemented.");
            }

            var p = new XElementParser(element);
            p.IgnoreElement("annotation");

            if (p.ParseElement("simpleType") ||
                p.ParseElement("complexType", x =>
                {
                    var complexTypeName = $"{element.Attribute("name").Value}Type";
                    fragment = new ComplexTypeFragment(element.Attribute("name").Value, complexTypeName, element.IsOptional(), x);

                    var newType = fragment.BuildTypeDeclaration(referencedTypes);
                    if (newType != null)
                        modifiedType = modifiedType.AddMembers(newType);
                }))
            { }

            while (p.ParseElement("unique") ||
                    p.ParseElement("key") ||
                    p.ParseElement("keyref"))
            { }

            if (fragment != null)
                modifiedType = modifiedType.AddMembers(fragment.PropertyFragment.BuildPropertyDeclaration());

            type = modifiedType;
        }

        private void ParseComplexContent(XElement element, IDictionary<XmlQualifiedName, bool> referencedTypes, ref ClassDeclarationSyntax type)
        {
            var modifiedType = type;

            var isMixed = (element.Attribute("mixed")?.AsBoolean()).GetValueOrDefault(false);
            if (isMixed)
                throw new NotImplementedException("ComplexType with mixed complex content is not supported.");

            var p = new XElementParser(element);
            p.IgnoreElement("annotation");

            if (!p.ParseElement("restriction") &&
                !p.ParseElement("extension", x => ParseExtension(x, referencedTypes, ref modifiedType)))
                throw new XmlException("ComplexType complex content should define `restriction` or `extension` element.");

            p.ThrowIfNotDone();

            type = modifiedType;
        }

        private void ParseExtension(XElement element, IDictionary<XmlQualifiedName, bool> referencedTypes, ref ClassDeclarationSyntax type)
        {
            var modifiedType = type;

            var baseTypeName = element.Attribute("base")?.AsXName();
            if (baseTypeName == null)
                throw new XmlException("Extension elements must have `base` attribute specified.");

            CodeFragmentFactory.ReferenceType(referencedTypes, baseTypeName);

            modifiedType = modifiedType.AddBaseListTypes(SimpleBaseType(ParseTypeName(baseTypeName.LocalName)));

            var p = new XElementParser(element);
            p.IgnoreElement("annotation");

            if (p.ParseElement("group") ||
                p.ParseElement("all") ||
                p.ParseElement("choice") ||
                p.ParseElement("sequence", x => ParseSequence(x, referencedTypes, ref modifiedType)))
            { }

            while (p.ParseElement("attribute") ||
                   p.ParseElement("attributeGroup"))
            { }

            if (p.ParseElement("anyAttribute"))
            { }

            p.ThrowIfNotDone();

            type = modifiedType;
        }
    }
}