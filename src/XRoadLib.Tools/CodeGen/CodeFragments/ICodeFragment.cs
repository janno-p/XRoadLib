using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public interface ICodeFragment
    {
        PropertyDeclarationSyntax BuildPropertyDeclaration();
        SyntaxList<StatementSyntax> BuildSerializationStatements();
        SyntaxList<StatementSyntax> BuildDeserializationStatements();
    }
}