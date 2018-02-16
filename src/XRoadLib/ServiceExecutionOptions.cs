using System;
using XRoadLib.Events;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib
{
    public class ServiceExecutionOptions
    {
        public string RequestNamespace { get; set; }
        public IServiceMap ServiceMap { get; set; }
        public string OperationName { get; set; }
        public uint? Version { get; set; }
        public EventHandler<XRoadRequestEventArgs> BeforeRequest;
        public EventHandler<XRoadResponseEventArgs> BeforeDeserialize;
    }
}