using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using XRoadLib.Attributes;
using XRoadLib.Configuration;
using XRoadLib.Header;

namespace XRoadLib.Extensions
{
    public static class XRoadProtocolExtensions
    {
        private static readonly Regex namespacePatternV20 = new Regex(@"(http\:\/\/producers.\w.xtee.riik.ee/producer/\w)/.+");
        private static readonly Regex namespacePatternV31 = new Regex(@"(http://\w.x-road.ee/producer/).+");
        private static readonly Regex namespacePatternV40 = new Regex(@"(http://\w.x-road.eu/producer)/.+");

        private static void ConstrainToDefinedValue(XRoadProtocol protocol)
        {
            if (!protocol.HasDefinedValue())
                throw new ArgumentOutOfRangeException(nameof(protocol));
        }

        public static bool HasValidValue(this XRoadProtocol protocol)
        {
            return Enum.IsDefined(typeof(XRoadProtocol), protocol);
        }

        public static bool HasDefinedValue(this XRoadProtocol protocol)
        {
            return protocol.HasValidValue() && protocol != XRoadProtocol.Undefined;
        }

        public static bool DefinesHeadersForNamespace(this XRoadProtocol protocol, string ns)
        {
            ConstrainToDefinedValue(protocol);

            switch (ns)
            {
                case NamespaceConstants.XTEE:
                    return protocol == XRoadProtocol.Version20;

                case NamespaceConstants.XROAD:
                    return protocol == XRoadProtocol.Version31;

                case NamespaceConstants.XROAD_V4:
                case NamespaceConstants.XROAD_V4_REPR:
                    return protocol == XRoadProtocol.Version40;

                default:
                    return false;
            }
        }

        public static string GetNamespace(this XRoadProtocol protocol)
        {
            ConstrainToDefinedValue(protocol);

            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    return NamespaceConstants.XTEE;

                case XRoadProtocol.Version31:
                    return NamespaceConstants.XROAD;

                case XRoadProtocol.Version40:
                    return NamespaceConstants.XROAD_V4;

                default:
                    throw new ArgumentException($"Unmapped X-Road protocol version `{protocol}`.", nameof(protocol));
            }
        }

        public static string GetProducerNamespace(this XRoadProtocol protocol, string producerName)
        {
            ConstrainToDefinedValue(protocol);

            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    return $"http://producers.{producerName}.xtee.riik.ee/producer/{producerName}";

                case XRoadProtocol.Version31:
                    return $"http://{producerName}.x-road.ee/producer/";

                case XRoadProtocol.Version40:
                    return $"http://{producerName}.x-road.eu/producer";

                default:
                    throw new ArgumentException($"Unmapped X-Road protocol version `{protocol}`.", nameof(protocol));
            }
        }

        public static string GetProducerNamespaceBase(this XRoadProtocol protocol, string ns)
        {
            ConstrainToDefinedValue(protocol);

            Regex regex;

            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    regex = namespacePatternV20;
                    break;

                case XRoadProtocol.Version31:
                    regex = namespacePatternV31;
                    break;

                case XRoadProtocol.Version40:
                    regex = namespacePatternV40;
                    break;

                default:
                    throw new ArgumentException($"Unmapped X-Road protocol version `{protocol}`.", nameof(protocol));
            }

            var match = regex.Match(ns);

            return match.Success ? match.Groups[1].Value : null;
        }

        public static string GetPrefix(this XRoadProtocol protocol)
        {
            ConstrainToDefinedValue(protocol);

            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    return PrefixConstants.XTEE;

                case XRoadProtocol.Version31:
                case XRoadProtocol.Version40:
                    return PrefixConstants.XROAD;

                default:
                    throw new ArgumentException($"Unmapped X-Road protocol version `{protocol}`.", nameof(protocol));
            }
        }

        internal static XRoadHeaderBase CreateXRoadHeader(this XRoadProtocol protocol)
        {
            if (!protocol.HasValidValue())
                throw new ArgumentOutOfRangeException(nameof(protocol));

            switch (protocol)
            {
                case XRoadProtocol.Undefined:
                    return null;

                case XRoadProtocol.Version20:
                    return new XRoadHeader20();

                case XRoadProtocol.Version31:
                    return new XRoadHeader31();

                case XRoadProtocol.Version40:
                    return new XRoadHeader40();

                default:
                    throw new ArgumentException($"Unmapped X-Road protocol version `{protocol}`.", nameof(protocol));
            }
        }

        private static XRoadSupportedProtocolAttribute FindSupportedProtocolAttribute(this XRoadProtocol protocol, Assembly contractAssembly)
        {
            return contractAssembly.GetCustomAttributes(typeof(XRoadSupportedProtocolAttribute), false)
                                   .OfType<XRoadSupportedProtocolAttribute>()
                                   .SingleOrDefault(attr => attr.Protocol == protocol);
        }

        private static XRoadSupportedProtocolAttribute GetSupportedProtocolAttribute(this XRoadProtocol protocol, Assembly contractAssembly)
        {
            var attribute = protocol.FindSupportedProtocolAttribute(contractAssembly);
            if (attribute == null)
                throw new Exception($"Assembly `{contractAssembly.GetName().Name}` does not offer contract for X-Road messaging protocol version `{protocol}`.");

            return attribute;
        }

        public static IXRoadContractConfiguration GetContractConfiguration(this XRoadProtocol protocol, Assembly contractAssembly)
        {
            var attribute = protocol.GetSupportedProtocolAttribute(contractAssembly);

            if (attribute.Configuration != null && !typeof(IXRoadContractConfiguration).IsAssignableFrom(attribute.Configuration))
                throw new ArgumentException("XRoadSupportedProtocolAttribute Configuration type should implement `IXRoadContractConfiguration` interface.");

            return attribute.Configuration != null ? (IXRoadContractConfiguration)Activator.CreateInstance(attribute.Configuration) : null;
        }
    }
}