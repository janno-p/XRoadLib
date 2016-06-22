using System;
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
            var orderValue = x.Order.CompareTo(y.Order);
            if (orderValue != 0)
                return orderValue;

            var xNamespace = x.Name?.NamespaceName ?? x.ArrayItemDefinition?.Name?.NamespaceName ?? "";
            var yNamespace = y.Name?.NamespaceName ?? y.ArrayItemDefinition?.Name?.NamespaceName ?? "";

            var namespaceValue = string.Compare(xNamespace, yNamespace);
            if (namespaceValue != 0)
                return namespaceValue;

            var xName = x.Name?.LocalName ?? x.ArrayItemDefinition?.Name?.LocalName ?? "";
            var yName = y.Name?.LocalName ?? y.ArrayItemDefinition?.Name?.LocalName ?? "";

            return string.Compare(xName, yName);
        }
    }
}