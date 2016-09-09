using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class DateTimeFragment : SimpleTypeFragment
    {
        public DateTimeFragment(string elementName, bool isNullable, bool isOptional)
            : base(elementName, isOptional, isNullable, ParseTypeName("DateTime"))
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