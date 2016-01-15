using System;
using XRoadLib.Extensions;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XRoadSupportedProtocolAttribute : Attribute
    {
        /// <summary>
        /// X-Road messaging protocol version that is supported by this instance of the attribute.
        /// </summary>
        public XRoadProtocol Protocol { get; }

        /// <summary>
        /// X-Road producer name for this supported protocol version.
        /// </summary>
        public string ProducerName { get; }

        /// <summary>
        /// Type that implements <see cref="XRoadLib.Configuration.IXRoadContractConfiguration" /> interface
        /// to provide overrides for default behavior of serialization and producer definition generation.
        /// </summary>
        public Type Configuration { get; set; }

        public XRoadSupportedProtocolAttribute(XRoadProtocol protocol, string producerName)
        {
            if (!protocol.HasDefinedValue())
                throw new ArgumentException($"Supported protcol value `{protocol}` does not meet any valid X-Road messaging protocol version.", nameof(protocol));
            Protocol = protocol;

            if (string.IsNullOrWhiteSpace(producerName))
                throw new ArgumentNullException(nameof(producerName));
            ProducerName = producerName;
        }
    }
}