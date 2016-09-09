using System.Xml.Linq;

namespace XRoadLib.Tools.CodeGen.Extensions
{
    public static class XElementExtensions
    {
        public static XName GetAttributeAsXName(this XElement element, XName attributeName)
        {
            var typeAttribute = element.Attribute(attributeName);
            if (typeAttribute == null)
                return null;

            var parts = typeAttribute.Value.Split(new[] { ':' }, 2);
            var ns = parts.Length == 1 ? element.GetNamespaceOfPrefix("") : element.GetNamespaceOfPrefix(parts[0]);
            var name = parts.Length == 1 ? parts[0] : parts[1];

            return XName.Get(name, ns.NamespaceName);
        }
    }
}