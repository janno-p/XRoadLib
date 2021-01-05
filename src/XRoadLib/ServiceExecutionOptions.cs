using System;
using System.Diagnostics.CodeAnalysis;
using XRoadLib.Events;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ServiceExecutionOptions
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string RequestNamespace { get; set; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public IServiceMap ServiceMap { get; set; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string OperationName { get; set; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public uint? Version { get; set; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public IMessageFormatter MessageFormatter { get; set; }

        [SuppressMessage("ReSharper", "UnassignedField.Global")]
        public EventHandler<XRoadRequestEventArgs> BeforeRequest;

        [SuppressMessage("ReSharper", "UnassignedField.Global")]
        public EventHandler<XRoadResponseEventArgs> BeforeDeserialize;
    }
}