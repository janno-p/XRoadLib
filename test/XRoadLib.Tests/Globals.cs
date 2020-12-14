using System;
using System.IO;
using XRoadLib.Schema;
using XRoadLib.Tests.Contract;

namespace XRoadLib.Tests
{
    public static class Globals
    {
        public static ServiceManager ServiceManager { get; } = new ServiceManager("4.0", new DefaultSchemaExporter("http://test-producer.x-road.eu/", typeof(Class1).Assembly) { SupportedVersions = { 1u, 2u, 3u } });
        public static DirectoryInfo StoragePath = new DirectoryInfo(Path.GetTempPath());

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