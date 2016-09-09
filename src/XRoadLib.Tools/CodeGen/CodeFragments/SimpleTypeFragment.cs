using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XRoadLib.Tools.CodeGen.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public abstract class SimpleTypeFragment : ICodeFragment
    {
        protected readonly bool isOptional;
        protected readonly string elementName;
        protected readonly SyntaxToken propertyNameToken;
        protected readonly TypeSyntax typeSyntax;

        protected SimpleTypeFragment(string elementName, bool isOptional, TypeSyntax typeSyntax)
        {
            this.elementName = elementName;
            this.isOptional = isOptional;
            this.propertyNameToken = Identifier(elementName);
            this.typeSyntax = typeSyntax;
        }

        public abstract SyntaxList<StatementSyntax> BuildDeserializationStatements();

        public PropertyDeclarationSyntax BuildPropertyDeclaration()
        {
            return SyntaxHelper.BuildProperty(typeSyntax, isOptional, propertyNameToken);
        }

        public abstract SyntaxList<StatementSyntax> BuildSerializationStatements();

        public ClassDeclarationSyntax BuildTypeDeclaration()
        {
            throw new NotImplementedException();
        }
    }
}