using System.IO;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.Http
{
    public class ServiceExecutionOptions
    {
        public string RequestNamespace { get; set; }
        public IMessageFormatter MessageFormatter { get; set; }
        public IServiceMap ServiceMap { get; set; }
        public string OperationName { get; set; }
        public uint? Version { get; set; }
        public DirectoryInfo StoragePath { get; set; }
    }
}