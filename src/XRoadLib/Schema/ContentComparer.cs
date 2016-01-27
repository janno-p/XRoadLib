using System;
using System.Collections.Generic;

namespace XRoadLib.Schema
{
    public abstract class ContentComparer<T> : IComparer<ContentDefinition<T>>
    {
        public int Compare(ContentDefinition<T> x, ContentDefinition<T> y)
        {
            var orderValue = x.Order.CompareTo(y.Order);
            if (orderValue != 0)
                return orderValue;

            var namespaceValue = string.Compare(x.Name.NamespaceName, y.Name.NamespaceName, StringComparison.InvariantCulture);
            if (namespaceValue != 0)
                return namespaceValue;

            return string.Compare(x.Name.LocalName, y.Name.LocalName, StringComparison.InvariantCulture);
        }
    }
}