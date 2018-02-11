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

            var xName = x.SerializedName;
            var yName = y.SerializedName;

            var namespaceValue = string.CompareOrdinal(xName?.NamespaceName ?? "", yName?.NamespaceName ?? "");

            return namespaceValue == 0
                ? string.CompareOrdinal(xName?.LocalName ?? "", yName?.LocalName ?? "")
                : namespaceValue;
        }
    }
}