using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Configuration
{
    public interface ITypeConfigurationProvider
    {
        XName GetTypeName(Type type);

        XRoadContentLayoutMode GetContentLayoutMode(Type type);

        IComparer<PropertyInfo> GetPropertyComparer(Type type);
    }
}