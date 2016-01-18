using System;
using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Extensions;

namespace XRoadLib.Configuration
{
    internal class DefaultComparer : IComparer<PropertyInfo>
    {
        internal readonly static IComparer<PropertyInfo> Instance = new DefaultComparer();

        public int Compare(PropertyInfo x, PropertyInfo y)
        {
            return string.Compare(x.GetPropertyName(), y.GetPropertyName(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}