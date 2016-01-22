using System;
using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Extensions;

namespace XRoadLib.Configuration
{
    public abstract class TypeConfigurationBase : ITypeConfiguration
    {
        private readonly IComparer<PropertyInfo> defaultComparer;

        protected TypeConfigurationBase()
        {
            defaultComparer = new DefaultComparer(this);
        }

        public virtual string GetPropertyName(PropertyInfo propertyInfo)
        {
            return null;
        }

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
            return defaultComparer;
        }
    }
}