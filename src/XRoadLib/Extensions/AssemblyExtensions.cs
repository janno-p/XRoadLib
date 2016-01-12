﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions
{
    public static class AssemblyExtensions
    {
        public static string FindProducerName(this Assembly assembly)
        {
            return assembly.GetSingleAttribute<XRoadProducerNameAttribute>()?.Value;
        }

        public static string GetProducerName(this Assembly assembly)
        {
            var producerName = assembly.FindProducerName();

            if (string.IsNullOrWhiteSpace(producerName))
                throw new Exception($"Unable to extract producer name from contract assembly `{assembly.GetName().Name}`.");

            return producerName;
        }

        public static XRoadProducerConfigurationAttribute GetConfigurationAttribute(this Assembly assembly, XRoadProtocol protocol)
        {
            return assembly.GetAppliableAttribute<XRoadProducerConfigurationAttribute>(protocol);
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

        internal static bool HasStrictOperationSignature(this Assembly assembly, XRoadProtocol protocol)
        {
            return assembly.GetConfigurationAttribute(protocol)?.StrictOperationSignature ?? true;
        }
    }
}