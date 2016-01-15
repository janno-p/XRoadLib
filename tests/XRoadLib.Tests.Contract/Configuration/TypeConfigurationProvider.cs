using System;
using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Configuration;
using XRoadLib.Tests.Contract.Comparers;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class TypeConfigurationProvider : TypeConfigurationProviderBase
    {
        private static readonly IComparer<PropertyInfo> orderComparer = new OrderComparer();

        public override XRoadContentLayoutMode GetContentLayoutMode(Type type)
        {
            if (type == typeof(ParamType1))
                return XRoadContentLayoutMode.Flexible;

            return base.GetContentLayoutMode(type);
        }

        public override IComparer<PropertyInfo> GetPropertyComparer(Type type)
        {
            if (type == typeof(TestDto))
                return orderComparer;

            return base.GetPropertyComparer(type);
        }
    }
}