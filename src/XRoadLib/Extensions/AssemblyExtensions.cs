using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions
{
    public static class AssemblyExtensions
    {
        public static string FindProducerName(this Assembly assembly, XRoadProtocol protocol)
        {
            var attribute = assembly.GetConfigurationAttribute(protocol);
            return attribute != null ? attribute.ProducerName : null;
        }

        public static string GetProducerName(this Assembly assembly, XRoadProtocol protocol)
        {
            var producerName = assembly.FindProducerName(protocol);

            if (string.IsNullOrWhiteSpace(producerName))
                throw new Exception($"Unable to extract producer name from contract assembly `{assembly.GetName().Name}` for protocol `{protocol.ToString()}`.");

            return producerName;
        }

        public static string GetProducerNamespace(this Assembly assembly, XRoadProtocol protocol)
        {
            var producerName = assembly.GetProducerName(protocol);
            return NamespaceHelper.GetProducerNamespace(producerName, protocol);
        }

        internal static XRoadProducerConfigurationAttribute GetConfigurationAttribute(this Assembly assembly, XRoadProtocol protocol)
        {
            var attributes = assembly.GetCustomAttributes(typeof(XRoadProducerConfigurationAttribute), false)
                                     .OfType<XRoadProducerConfigurationAttribute>()
                                     .ToList();

            return attributes.SingleOrDefault(attr => attr.appliesTo.HasValue && attr.appliesTo.Value == protocol)
                ?? attributes.SingleOrDefault(attr => !attr.appliesTo.HasValue);
        }

        public static uint? GetXRoadPublishedVersion(this Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(XRoadProducerConfigurationAttribute), false)
                           .OfType<XRoadProducerConfigurationAttribute>()
                           .Select(x => (uint?)x.MaxOperationVersion)
                           .SingleOrDefault();
        }

        public static IDictionary<MethodInfo, IDictionary<string, XRoadServiceAttribute>> GetServiceContracts(this Assembly assembly)
        {
            return assembly.GetTypes()
                           .Where(t => t.IsInterface)
                           .SelectMany(t => t.GetMethods())
                           .Select(m => Tuple.Create(m, m.GetCustomAttributes(typeof(XRoadServiceAttribute), false)
                                                         .OfType<XRoadServiceAttribute>()
                                                         .ToDictionary(x => x.Name, x => x)))
                           .Where(x => x.Item2.Any())
                           .ToDictionary(x => x.Item1, x => (IDictionary<string, XRoadServiceAttribute>)x.Item2);
        }
    }
}