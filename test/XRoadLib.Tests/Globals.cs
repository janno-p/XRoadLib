using System;
using System.Reflection;
using System.Xml.Linq;
using XRoadLib.Schema;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Configuration;

namespace XRoadLib.Tests
{
    public static class Globals
    {
        public static XRoadProtocol XRoadProtocol20 { get; }
        public static XRoadProtocol XRoadProtocol31 { get; }
        public static XRoadProtocol XRoadProtocol40 { get; }

        static Globals()
        {
            var protocol20 = new XRoadProtocol("2.0", new CustomSchemaExporterXRoad20());
            protocol20.SetContractAssembly(typeof(Class1).GetTypeInfo().Assembly, null, 1u, 2u, 3u);
            XRoadProtocol20 = protocol20;

            var protocol31 = new XRoadProtocol("3.1", new CustomSchemaExporterXRoad31());
            protocol31.SetContractAssembly(typeof(Class1).GetTypeInfo().Assembly, null, 1u, 2u, 3u);
            XRoadProtocol31 = protocol31;

            var protocol = new XRoadProtocol("4.0", "http://test-producer.x-road.eu/");
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
            public bool IgnoreExplicitType { get; }
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