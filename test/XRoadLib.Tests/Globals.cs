using System;
using System.Reflection;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Configuration;

namespace XRoadLib.Tests
{
    public static class Globals
    {
        public static ServiceManager<XRoadHeader20> ServiceManager20 { get; } = new ServiceManager<XRoadHeader20>("2.0", new CustomSchemaExporterXRoad20());
        public static ServiceManager<XRoadHeader31> ServiceManager31 { get; } = new ServiceManager<XRoadHeader31>("3.1", new CustomSchemaExporterXRoad31());
        public static ServiceManager<XRoadHeader40> ServiceManager40 { get; } = new ServiceManager<XRoadHeader40>("4.0", new DefaultSchemaExporter("http://test-producer.x-road.eu/", typeof(Class1).GetTypeInfo().Assembly) { SupportedVersions = { 1u, 2u } });

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