using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetXRoadProducerName(this Assembly assembly)
        {
            var producerName = assembly.FindXRoadProducerName();

            if (string.IsNullOrWhiteSpace(producerName))
                throw new Exception($"Unable to extract producer name from contract assembly `{assembly.FullName}`");

            return producerName;
        }

        public static string FindXRoadProducerName(this Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(XRoadProducerNameAttribute), false)
                           .OfType<XRoadProducerNameAttribute>()
                           .Select(x => x.Value)
                           .SingleOrDefault();
        }

        public static uint? GetXRoadPublishedVersion(this Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(XRoadPublishedVersionAttribute), false)
                           .OfType<XRoadPublishedVersionAttribute>()
                           .Select(x => (uint?)x.Version)
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