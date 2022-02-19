using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Tests.Contract;

namespace XRoadLib.Tests;

public static class Globals
{
    public static ServiceManager<XRoadHeader> ServiceManager { get; } = new("4.0", new DefaultSchemaExporter("http://test-producer.x-road.eu/", typeof(Class1).Assembly, new[] { 1u, 2u, 3u }));

    public static ContentDefinition GetTestDefinition(Type type)
    {
        return new TestDefinition(type);
    }

    private sealed class TestDefinition : ContentDefinition
    {
        public TestDefinition(Type type)
            : base(new TestParticleDefinition())
        {
            RuntimeType = type;
        }
    }

    private sealed class TestParticleDefinition : ParticleDefinition
    { }
}