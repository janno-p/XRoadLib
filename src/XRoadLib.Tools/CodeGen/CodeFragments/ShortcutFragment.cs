using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public class ShortcutFragment : ICodeFragment
    {
        public IPropertyFragment PropertyFragment { get; }

        public ShortcutFragment(string elementName, TypeSyntax baseType, bool isOptional)
        {
            PropertyFragment = new PropertyFragment(baseType, elementName, false, isOptional);
        }

        public SyntaxList<StatementSyntax> BuildSerializationStatements()
        {
            throw new NotImplementedException();
        }

        public SyntaxList<StatementSyntax> BuildDeserializationStatements()
        {
            throw new NotImplementedException();
        }

        public ClassDeclarationSyntax BuildTypeDeclaration(IDictionary<XmlQualifiedName, bool> referencedTypes)
        {
            throw new NotImplementedException();
        }
    }
}