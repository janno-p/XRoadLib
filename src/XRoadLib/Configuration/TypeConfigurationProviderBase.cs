using System;
using System.Collections.Generic;
using System.Reflection;

namespace XRoadLib.Configuration
{
    public class TypeConfigurationProviderBase : ITypeConfigurationProvider
    {
        public virtual string GetTypeName(Type type)
        {
            return type.Name;
        }

        public virtual XRoadContentLayoutMode GetContentLayoutMode(Type type)
        {
            return XRoadContentLayoutMode.Strict;
        }

        public virtual IComparer<PropertyInfo> GetPropertyComparer(Type type)
        {
            return null;
        }
    }
}