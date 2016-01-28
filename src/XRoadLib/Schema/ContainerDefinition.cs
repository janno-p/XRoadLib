using System.Collections.Generic;
using System.Reflection;

namespace XRoadLib.Schema
{
    public abstract class ContainerDefinition<TRuntimeInfo, TElementDefinition> : Definition<TRuntimeInfo>
        where TRuntimeInfo : ICustomAttributeProvider
    {
        public bool HasStrictContentOrder { get; set; }

        public IComparer<TElementDefinition> ContentComparer { get; set; }
    }
}