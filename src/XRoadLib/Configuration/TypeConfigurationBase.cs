using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Configuration
{
    public abstract class TypeConfigurationBase : ITypeConfiguration
    {
        private readonly IComparer<PropertyInfo> defaultComparer;

        protected TypeConfigurationBase()
        {
            defaultComparer = new DefaultComparer(this);
        }

        public virtual bool? GetPropertyIsNullable(PropertyInfo propertyInfo, bool isArrayItem)
        {
            return null;
        }

        public virtual string GetPropertyName(PropertyInfo propertyInfo)
        {
            return null;
        }

        public virtual XName GetTypeName(Type type)
        {
            return null;
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