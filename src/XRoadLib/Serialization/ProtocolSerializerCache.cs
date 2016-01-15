using System;
using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Extensions;

namespace XRoadLib.Serialization
{
    public class ProtocolSerializerCache : IProtocolSerializerCache
    {
        private readonly IDictionary<XRoadProtocol, ISerializerCache> serializerCaches = new Dictionary<XRoadProtocol, ISerializerCache>();

        public Assembly ContractAssembly { get; }
        public string ProducerName { get; }

        public ProtocolSerializerCache(Assembly contractAssembly, params XRoadProtocol[] protocols)
        {
            if (contractAssembly == null)
                throw new ArgumentNullException(nameof(contractAssembly));
            ContractAssembly = contractAssembly;

            ProducerName = contractAssembly.GetProducerName();

            if (protocols == null || protocols.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(protocols));

            foreach (var protocol in protocols)
                serializerCaches.Add(protocol, new SerializerCache(ContractAssembly, protocol));
        }

        public ISerializerCache GetSerializerCache(XRoadProtocol protocol)
        {
            ISerializerCache serializerCache;
            if (serializerCaches.TryGetValue(protocol, out serializerCache))
                return serializerCache;

            throw XRoadException.InvalidQuery("Unsupported protocol version `{0}`.", protocol);
        }
    }
}