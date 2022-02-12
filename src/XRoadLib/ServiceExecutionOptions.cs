using XRoadLib.Events;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib;

[UsedImplicitly]
public class ServiceExecutionOptions
{
    [UsedImplicitly]
    public string RequestNamespace { get; set; }

    [UsedImplicitly]
    public IServiceMap ServiceMap { get; set; }

    [UsedImplicitly]
    public string OperationName { get; set; }

    [UsedImplicitly]
    public uint? Version { get; set; }

    [UsedImplicitly]
    public IMessageFormatter MessageFormatter { get; set; }

    [UsedImplicitly]
    public EventHandler<XRoadRequestEventArgs> BeforeRequest { get; set; }

    [UsedImplicitly]
    public EventHandler<XRoadResponseEventArgs> BeforeDeserialize { get; set; }
}