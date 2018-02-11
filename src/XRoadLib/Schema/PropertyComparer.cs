using System.Collections.Generic;

namespace XRoadLib.Schema
{
    public class PropertyComparer : IComparer<PropertyDefinition>
    {
        public static PropertyComparer Instance { get; } = new PropertyComparer();

        private PropertyComparer()
        { }

        public int Compare(PropertyDefinition x, PropertyDefinition y)
        {
            return CompareContent(x?.Content, y?.Content);
        }

        private static int CompareContent(ContentDefinition x, ContentDefinition y)
        {
            var orderValue = x.Order.CompareTo(y.Order);
            if (orderValue != 0)
                return orderValue;

            var xNamespace = x.Name?.NamespaceName ?? x.ArrayItemDefinition?.Name?.NamespaceName ?? "";
            var yNamespace = y.Name?.NamespaceName ?? y.ArrayItemDefinition?.Name?.NamespaceName ?? "";

            var namespaceValue = string.CompareOrdinal(xNamespace, yNamespace);
            if (namespaceValue != 0)
                return namespaceValue;

            var xName = x.Name?.LocalName ?? x.ArrayItemDefinition?.Name?.LocalName ?? "";
            var yName = y.Name?.LocalName ?? y.ArrayItemDefinition?.Name?.LocalName ?? "";

            return string.CompareOrdinal(xName, yName);
        }
    }
}