using System.Reflection;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions;

public static class AssemblyExtensions
{
    public static IDictionary<MethodInfo, IList<XRoadServiceAttribute>> GetServiceContracts(this Assembly assembly)
    {
        return assembly.GetTypes()
                       .Where(t => t.GetTypeInfo().IsInterface)
                       .SelectMany(t => t.GetTypeInfo().GetMethods())
                       .Select(m => Tuple.Create(m, m.GetCustomAttributes(typeof(XRoadServiceAttribute), false)
                                                     .OfType<XRoadServiceAttribute>()
                                                     .ToList()))
                       .Where(x => x.Item2.Any())
                       .ToDictionary(x => x.Item1, x => (IList<XRoadServiceAttribute>)x.Item2);
    }
}