using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class ReferencedTypeFragment : ICodeFragment
    {
        private readonly bool isOptional;
        private readonly SyntaxToken propertyNameToken;
        private readonly TypeSyntax typeSyntax;

        public IPropertyFragment PropertyFragment { get; }

        public ReferencedTypeFragment(string elementName, string typeName, bool isOptional)
        {
            this.isOptional = isOptional;
            this.propertyNameToken = Identifier(elementName);
            this.typeSyntax = ParseTypeName(typeName);

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
            throw new NotImplementedException();
        }
    }
}