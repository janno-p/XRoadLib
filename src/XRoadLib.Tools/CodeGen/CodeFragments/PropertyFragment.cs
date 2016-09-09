using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XRoadLib.Tools.CodeGen.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class PropertyFragment : IPropertyFragment
    {
        private readonly bool isNullable;
        private readonly bool isOptional;
        private readonly string propertyName;
        private readonly TypeSyntax propertyType;

        public PropertyFragment(TypeSyntax propertyType, string propertyName, bool isNullable, bool isOptional)
        {
            this.isNullable = isNullable;
            this.isOptional = isOptional;
            this.propertyName = propertyName;
            this.propertyType = propertyType;
        }

        public PropertyDeclarationSyntax BuildPropertyDeclaration()
        {
            var typeSyntax = propertyType;

            if (isNullable)
                typeSyntax = NullableType(typeSyntax);

            if (isOptional)
                typeSyntax = typeSyntax.AsOptionalType();

            var propertyDeclaration = PropertyDeclaration(typeSyntax, Identifier(propertyName))
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            return propertyDeclaration
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }
    }
}