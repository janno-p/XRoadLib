using System;
using System.Reflection;
using XRoadLib.Schema;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Configuration;

namespace XRoadLib.Tests
{
    public static class Globals
    {
        public static IXRoadProtocol XRoadProtocol20 { get; } = new XRoadProtocol("2.0", new CustomSchemaExporterXRoad20());
        public static IXRoadProtocol XRoadProtocol31 { get; } = new XRoadProtocol("3.1", new CustomSchemaExporterXRoad31());
        public static IXRoadProtocol XRoadProtocol40 { get; } = new XRoadProtocol("4.0", new DefaultSchemaExporter("http://test-producer.x-road.eu/", typeof(Class1).GetTypeInfo().Assembly) { SupportedVersions = { 1u, 2u } });

        public static ContentDefinition GetTestDefinition(Type type)
        {
            return new TestDefinition(type);
        }

        private class TestDefinition : ContentDefinition
        {
            public TestDefinition(Type type)
                : base(null)
            {
                RuntimeType = type;
            }
        }
    }
}