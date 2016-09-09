using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.Extensions
{
    public static class SyntaxHelper
    {
        public static TypeSyntax TypeSyntax<T>()
        {
            return ParseTypeName(GetTypeName(typeof(T)));
        }

        private static string GetTypeName(Type type)
        {
            var name = type.Name.Replace("+", ".");
            if (name.Contains("`"))
                name = name.Substring(0, name.IndexOf("`"));
            return name;
        }

        public static PropertyDeclarationSyntax BuildProperty(TypeSyntax typeSyntax, bool isOptional, SyntaxToken propertyNameToken)
        {
            return PropertyDeclaration(isOptional ? typeSyntax.AsOptionalType() : typeSyntax, propertyNameToken)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }
    }
}