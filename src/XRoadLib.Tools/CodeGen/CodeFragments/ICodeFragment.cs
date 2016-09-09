using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public interface ICodeFragment
    {
        IPropertyFragment PropertyFragment { get; }

        SyntaxList<StatementSyntax> BuildSerializationStatements();
        SyntaxList<StatementSyntax> BuildDeserializationStatements();
        ClassDeclarationSyntax BuildTypeDeclaration(IDictionary<XmlQualifiedName, bool> referencedTypes);
    }
}