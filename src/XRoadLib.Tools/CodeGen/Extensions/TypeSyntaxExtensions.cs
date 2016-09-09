using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.Extensions
{
    public static class TypeSyntaxExtensions
    {
        public static TypeSyntax AsOptionalType(this TypeSyntax type)
        {
            return GenericName(Identifier("Option"), TypeArgumentList(SeparatedList(new[] { type })));
        }
    }
}