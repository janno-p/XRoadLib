using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public interface IPropertyFragment
    {
        PropertyDeclarationSyntax BuildPropertyDeclaration(bool isCollection);
    }
}