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
        public static XRoadProtocol XRoadProtocol20 { get; } = new XRoadProtocol("2.0", new CustomSchemaExporterXRoad20());
        public static XRoadProtocol XRoadProtocol31 { get; } = new XRoadProtocol("3.1", new CustomSchemaExporterXRoad31());
        public static XRoadProtocol XRoadProtocol40 { get; } = new XRoadProtocol("4.0", new DefaultSchemaExporter("http://test-producer.x-road.eu/", typeof(Class1).GetTypeInfo().Assembly) { SupportedVersions = { 1u, 2u } });

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