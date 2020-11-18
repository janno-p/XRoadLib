using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions
{
    /// <summary>
    /// Extension methods for <see>IServiceMap</see>.
    /// </summary>
    public static class ServiceMapExtensions
    {
        /// <summary>
        /// Deserializes X-Road fault from message which is known to contain fault.
        /// </summary>
        public static async Task<IXRoadFault> DeserializeXRoadFaultAsync(this IServiceMap serviceMap, XRoadMessage message, IMessageFormatter messageFormatter)
        {
            message.ContentStream.Position = 0;

            using var reader = XmlReader.Create(message.ContentStream, new XmlReaderSettings { Async = true });

            await messageFormatter.MoveToPayloadAsync(reader, message.RootElementName).ConfigureAwait(false);

            var responseName = serviceMap.ResponseDefinition.Content.Name;
            if (!await reader.MoveToElementAsync(3, responseName).ConfigureAwait(false))
                throw new InvalidQueryException($"X-Road fault should be wrapped inside `{responseName}` element.");

            return await reader.ReadXRoadFaultAsync(4).ConfigureAwait(false);
        }
    }
}