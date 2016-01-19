using System;
using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Configuration;
using XRoadLib.Tests.Contract.Attributes;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class XRoadContractConfiguration : XRoadContractConfigurationBase
    {
        private static readonly IOperationConfiguration operationConfiguration = new OperationConfigurationImpl();
        private static readonly ITypeConfiguration typeConfiguration = new TypeConfigurationImpl();

        public override uint? MinOperationVersion => 1u;
        public override uint? MaxOperationVersion => 2u;
        public override string StandardHeaderName => "stdhdr";
        public override string PortTypeName => "TestProducerPortType";
        public override string BindingName => "TestBinding";
        public override string ServicePortName => "TestPort";
        public override string ServiceName => "TestService";
        public override string RequestTypeNameFormat => "{0}Request";
        public override string ResponseTypeNameFormat => "{0}Response";
        public override string RequestMessageNameFormat => "{0}";
        public override string ResponseMessageNameFormat => "{0}Response";
        public override IOperationConfiguration OperationConfiguration => operationConfiguration;
        public override ITypeConfiguration TypeConfiguration => typeConfiguration;

        private class OperationConfigurationImpl : OperationConfigurationBase
        {
            public override XRoadContentLayoutMode GetParameterLayout(MethodInfo methodInfo)
            {
                return XRoadContentLayoutMode.Strict;
            }
        }

        private class TypeConfigurationImpl : TypeConfigurationBase
        {
            private static readonly IComparer<PropertyInfo> orderComparer = new OrderComparer();

            public override XRoadContentLayoutMode GetContentLayoutMode(Type type)
            {
                return type == typeof(ParamType1) ? XRoadContentLayoutMode.Flexible : base.GetContentLayoutMode(type);
            }

            public override IComparer<PropertyInfo> GetPropertyComparer(Type type)
            {
                return type == typeof(TestDto) ? orderComparer : base.GetPropertyComparer(type);
            }
        }

        private class OrderComparer : IComparer<PropertyInfo>
        {
            public int Compare(PropertyInfo x, PropertyInfo y)
            {
                return x.GetCustomAttribute<OrderAttribute>().Value
                        .CompareTo(y.GetCustomAttribute<OrderAttribute>().Value);
            }
        }
    }
}