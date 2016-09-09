using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Tools.CodeGen.Extensions;

namespace XRoadLib.Tools.CodeGen.CodeFragments
{
    public static class CodeFragmentFactory
    {
        private static readonly IDictionary<string, Func<string, bool, bool, ICodeFragment>> builders = new Dictionary<string, Func<string, bool, bool, ICodeFragment>>
        {
            { "string", (nm, nil, opt) => new StringFragment(nm, opt) },
            { "date", (nm, nil, opt) => new DateFragment(nm, nil, opt) },
            { "dateTime", (nm, nil, opt) => new DateTimeFragment(nm, nil, opt) },
            { "long", (nm, nil, opt) => new LongFragment(nm, nil, opt) },
            { "base64Binary", (nm, nil, opt) => new StreamFragment(nm, opt) },
            { "int", (nm, nil, opt) => new IntFragment(nm, nil, opt) },
            { "decimal", (nm, nil, opt) => new DecimalFragment(nm, nil, opt) },
        };

        public static ICodeFragment GetElementFragment(XElement element, IDictionary<XmlQualifiedName, bool> referencedTypes)
        {
            var typeAttribute = element.Attribute("type");
            if (typeAttribute == null)
                return null;

            var typeName = typeAttribute.AsXName();

            Func<string, bool, bool, ICodeFragment> func;
            if (typeName.NamespaceName.Equals(NamespaceConstants.XSD) && builders.TryGetValue(typeName.LocalName, out func))
                return func(element.GetName(), element.IsNullable(), element.IsOptional());

            ReferenceType(referencedTypes, typeName);

            return new ReferencedTypeFragment(element.GetName(), typeName.LocalName, element.IsOptional());
        }

        public static void ReferenceType(IDictionary<XmlQualifiedName, bool> referencedTypes, XName typeName)
        {
            var qualifiedName = new XmlQualifiedName(typeName.LocalName, typeName.NamespaceName);
            if (!referencedTypes.ContainsKey(qualifiedName))
                referencedTypes[qualifiedName] = false;
        }
    }
}