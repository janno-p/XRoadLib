using System.Reflection;
using XRoadLib.Protocols;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomXRoad31Protocol : XRoad31Protocol
    {
        public static XRoadProtocol Instance { get; } = new CustomXRoad31Protocol();

        private CustomXRoad31Protocol()
            : base("test-producer", "http://test-producer.x-road.ee/producer/", null, new CustomSchemaExporterXRoad31())
        {
            SetContractAssembly(GetType().GetTypeInfo().Assembly, null, 1u, 2u, 3u);
        }
    }
}
