using System.Reflection;

namespace XRoadLib.Serialization
{
    public interface IProtocolSerializerCache
    {
        Assembly ContractAssembly { get; }

        ISerializerCache GetSerializerCache(XRoadProtocol protocol);
    }
}