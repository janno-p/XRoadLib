using System.Collections.Generic;

namespace XRoadLib.Schema
{
    public abstract class ContainerDefinition<TRuntimeInfo, TElementDefinition> : Definition<TRuntimeInfo>
    {
        public bool HasStrictContentOrder { get; set; }

        public IComparer<TElementDefinition> ContentComparer { get; set; }
    }
}