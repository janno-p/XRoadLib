using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class IntFragment : SimpleTypeFragment
    {
        public IntFragment(string elementName, bool isNullable, bool isOptional)
            : base(elementName, isOptional, isNullable, PredefinedType(Token(SyntaxKind.IntKeyword)))
        { }

        public override SyntaxList<StatementSyntax> BuildDeserializationStatements()
        {
            throw new NotImplementedException();
        }

        public override SyntaxList<StatementSyntax> BuildSerializationStatements()
        {
            throw new NotImplementedException();
        }
    }
}