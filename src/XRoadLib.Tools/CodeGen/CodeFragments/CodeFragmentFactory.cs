using System;
using System.Collections.Generic;
using System.Xml.Linq;
using XRoadLib.Tools.CodeGen.Extensions;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public static class CodeFragmentFactory
    {
        private static readonly IDictionary<string, Func<string, bool, ICodeFragment>> builders = new Dictionary<string, Func<string, bool, ICodeFragment>>
        {
            { "string", (nm, nil) => new StringFragment(nm, nil) },
            { "date", (nm, nil) => new DateFragment(nm, nil) },
            { "dateTime", (nm, nil) => new DateTimeFragment(nm, nil) },
            { "long", (nm, nil) => new LongFragment(nm, nil) },
            { "base64Binary", (nm, nil) => new StreamFragment(nm, nil) },
            { "int", (nm, nil) => new IntFragment(nm, nil) },
            { "decimal", (nm, nil) => new DecimalFragment(nm, nil) },
        };

        public static ICodeFragment GetElementFragment(XElement element)
        {
            var typeAttribute = element.Attribute("type");
            if (typeAttribute == null)
                return null;

            var typeName = typeAttribute.AsXName();

            Func<string, bool, ICodeFragment> func;
            if (typeName.NamespaceName.Equals(NamespaceConstants.XSD) && builders.TryGetValue(typeName.LocalName, out func))
                return func(element.Attribute("name").Value, element.IsOptional());

            throw new NotImplementedException(element.ToString());
        }

        /*
        private TypeSyntax GetPredefinedType(XElement element)
        {
            var typeName = element.GetAttributeAsXName("type");

            if (typeName.NamespaceName == NamespaceConstants.XSD)
                switch (typeName.LocalName)
                {
                    case "base64Binary": return ParseTypeName("Stream");
                    case "date": return ParseTypeName("DateTime");
                    case "long": return PredefinedType(Token(SyntaxKind.LongKeyword));
                    case "string": return PredefinedType(Token(SyntaxKind.StringKeyword));
                }

            throw new NotImplementedException(typeName.ToString());
        }
        */
    }
}