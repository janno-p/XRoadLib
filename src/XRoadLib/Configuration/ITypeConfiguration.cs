using System;
using System.Collections.Generic;
using System.Reflection;

namespace XRoadLib.Configuration
{
    public interface ITypeConfiguration
    {
        string GetPropertyName(PropertyInfo propertyInfo);

        string GetTypeName(Type type);

        XRoadContentLayoutMode GetContentLayoutMode(Type type);

        IComparer<PropertyInfo> GetPropertyComparer(Type type);
    }
}