using System.Reflection;
using XRoadLib.Protocols;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomXRoad20Protocol : XRoad20Protocol
    {
        private StringSerializationMode stringSerializationMode;

        public static XRoadProtocol Instance { get; } = new CustomXRoad20Protocol();

        public override StringSerializationMode StringSerializationMode => stringSerializationMode;

        public CustomXRoad20Protocol()
            : base("http://producers.test-producer.xtee.riik.ee/producer/test-producer", null, new CustomSchemaExporterXRoad20())
        {
            SetContractAssembly(GetType().GetTypeInfo().Assembly, null, 1u, 2u, 3u);
        }

        public void SetStringSerializationMode(StringSerializationMode stringSerializationMode)
        {
            this.stringSerializationMode = stringSerializationMode;
        }
    }
}
