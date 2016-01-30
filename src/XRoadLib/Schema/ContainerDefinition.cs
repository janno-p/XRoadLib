using System.Collections.Generic;

namespace XRoadLib.Schema
{
    public abstract class ContainerDefinition<TElementDefinition> : Definition
    {
        public bool HasStrictContentOrder { get; set; }

        public IComparer<TElementDefinition> ContentComparer { get; set; }
    }
}