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
        public static IXRoadFault DeserializeXRoadFault(this IServiceMap serviceMap, XRoadMessage message, IMessageFormatter messageFormatter)
        {
            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                messageFormatter.MoveToPayload(reader, message.RootElementName);

                var responseName = serviceMap.ResponseDefinition.Content.Name;
                if (!reader.MoveToElement(3, responseName))
                    throw new InvalidQueryException($"X-Road fault should be wrapped inside `{responseName}` element.");

                return reader.ReadXRoadFault(4);
            }
        }
    }
}