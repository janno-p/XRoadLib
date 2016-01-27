using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions
{
    public static class AssemblyExtensions
    {
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