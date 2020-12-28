using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<(Type OperationType, IList<XRoadOperationAttribute> Operations)> GetOperationContracts(this Assembly assembly)
        {
            return assembly.GetTypes()
                           .Where(t => t.IsXRoadOperation())
                           .Select(t => (t, (IList<XRoadOperationAttribute>)t.GetCustomAttributes<XRoadOperationAttribute>(false).ToList()))
                           .Where(x => x.Item2.Any());
        }
    }
}