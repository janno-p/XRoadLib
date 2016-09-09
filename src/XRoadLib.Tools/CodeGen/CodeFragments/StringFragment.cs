using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XRoadLib.Tools.CodeGen.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class StringFragment : ICodeFragment
    {
        private readonly bool isOptional;
        private readonly string elementName;
        private readonly SyntaxToken propertyNameToken;

        public StringFragment(string elementName, bool isOptional)
        {
            this.elementName = elementName;
            this.isOptional = isOptional;
            this.propertyNameToken = Identifier(elementName);
        }

        public SyntaxList<StatementSyntax> BuildDeserializationStatements()
        {
            throw new NotImplementedException();
        }

        public PropertyDeclarationSyntax BuildPropertyDeclaration()
        {
            TypeSyntax typeSyntax = PredefinedType(Token(SyntaxKind.StringKeyword));
            if (isOptional)
                typeSyntax = SyntaxHelper.GetOptionalType(typeSyntax);

            return PropertyDeclaration(typeSyntax, propertyNameToken)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }

        public SyntaxList<StatementSyntax> BuildSerializationStatements()
        {
            throw new NotImplementedException();
        }
    }
}