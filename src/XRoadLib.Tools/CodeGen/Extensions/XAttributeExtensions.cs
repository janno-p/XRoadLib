using System;
using System.Linq;
using System.Xml.Linq;

namespace XRoadLib.Tools.CodeGen.Extensions
{
    public static class XAttributeExtensions
    {
        public static XName AsXName(this XAttribute attribute)
        {
            var parts = attribute.Value.Split(new[] { ':' }, 2);
            var ns = parts.Length == 1 ? attribute.Parent.GetDefaultNamespace() : attribute.Parent.GetNamespaceOfPrefix(parts.First());
            var name = parts.Length == 1 ? parts[0] : parts[1];
            return XName.Get(name, ns.NamespaceName);
        }

        public static bool AsBoolean(this XAttribute attribute)
        {
            return Convert.ToBoolean(attribute.Value);
        }

        public static int AsInt32(this XAttribute attribute)
        {
            return attribute.Value.Equals("unbounded") ? int.MaxValue : Convert.ToInt32(attribute.Value);
        }
    }
}