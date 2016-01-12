using System.Reflection;

namespace XRoadLib.Serialization
{
    public interface IProtocolSerializerCache
    {
        Assembly ContractAssembly { get; }

        string ProducerName { get; }

        ISerializerCache GetSerializerCache(XRoadProtocol protocol);
    }
}