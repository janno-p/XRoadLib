using System;
using System.Reflection;
using System.Xml.Linq;
using XRoadLib.Protocols;
using XRoadLib.Schema;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Configuration;

namespace XRoadLib.Tests
{
    public static class Globals
    {
        public static XRoadProtocol XRoadProtocol20 { get; } = CustomXRoad20Protocol.Instance;
        public static XRoadProtocol XRoadProtocol31 { get; } = CustomXRoad31Protocol.Instance;
        public static XRoadProtocol XRoadProtocol40 { get; }

        static Globals()
        {
            var protocol = new XRoad40Protocol("http://test-producer.x-road.eu/");
            protocol.SetContractAssembly(typeof(Class1).GetTypeInfo().Assembly, null, 1u, 2u);
            XRoadProtocol40 = protocol;
        }

        public static IContentDefinition GetTestDefinition(Type type)
        {
            return new TestDefinition(type);
        }

        private class TestDefinition : IContentDefinition
        {
            public bool UseXop { get; }
            public Type RuntimeType { get; }
            public XName TypeName { get; }
            public ArrayItemDefinition ArrayItemDefinition { get; }
            public bool MergeContent { get; }

            public TestDefinition(Type type)
            {
                RuntimeType = type;
            }
        }
    }
}