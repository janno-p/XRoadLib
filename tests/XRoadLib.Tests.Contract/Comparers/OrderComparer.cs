using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Tests.Contract.Attributes;

namespace XRoadLib.Tests.Contract.Comparers
{
    internal class OrderComparer : IComparer<PropertyInfo>
    {
        public int Compare(PropertyInfo x, PropertyInfo y)
        {
            return x.GetCustomAttribute<OrderAttribute>().Value
                    .CompareTo(y.GetCustomAttribute<OrderAttribute>().Value);
        }
    }
}
