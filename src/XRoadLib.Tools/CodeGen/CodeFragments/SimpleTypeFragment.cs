using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public abstract class SimpleTypeFragment : ICodeFragment
    {
        protected readonly bool isNullable;
        protected readonly bool isOptional;
        protected readonly string elementName;
        protected readonly SyntaxToken propertyNameToken;
        protected readonly TypeSyntax typeSyntax;

        public IPropertyFragment PropertyFragment { get; }

        protected SimpleTypeFragment(string elementName, bool isOptional, bool isNullable, TypeSyntax typeSyntax)
        {
            this.elementName = elementName;
            this.isNullable = isNullable;
            this.isOptional = isOptional;
            this.propertyNameToken = Identifier(elementName);
            this.typeSyntax = typeSyntax;

            PropertyFragment = new PropertyFragment(typeSyntax, elementName, isNullable, isOptional);
        }

        public abstract SyntaxList<StatementSyntax> BuildDeserializationStatements();

        public abstract SyntaxList<StatementSyntax> BuildSerializationStatements();

        public ClassDeclarationSyntax BuildTypeDeclaration(IDictionary<XmlQualifiedName, bool> referencedTypes)
        {
            throw new NotImplementedException();
        }
    }
}