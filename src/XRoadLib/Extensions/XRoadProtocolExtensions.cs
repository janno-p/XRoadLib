using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions
{
    /// <summary>
    /// Extension methods of XRoadProtocol type.
    /// </summary>
    public static class XRoadProtocolExtensions
    {
        /// <summary>
        /// Initialize new X-Road message of this X-Road message protocol instance.
        /// </summary>
        public static XRoadMessage CreateMessage(this IXRoadProtocol protocol, IXRoadHeader header = null)
        {
            return new XRoadMessage(protocol, header ?? protocol.CreateHeader());
        }
    }
}