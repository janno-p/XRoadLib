using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class LongFragment : SimpleTypeFragment
    {
        public LongFragment(string elementName, bool isNullable, bool isOptional)
            : base(elementName, isOptional, isNullable, PredefinedType(Token(SyntaxKind.LongKeyword)))
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