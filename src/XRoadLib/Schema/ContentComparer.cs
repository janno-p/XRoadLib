using System;
using System.Collections.Generic;
using System.Reflection;

namespace XRoadLib.Schema
{
    public abstract class ContentComparer<T, TDefinition> : IComparer<TDefinition>
        where TDefinition : ContentDefinition<T>
        where T : ICustomAttributeProvider
    {
        public int Compare(TDefinition x, TDefinition y)
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