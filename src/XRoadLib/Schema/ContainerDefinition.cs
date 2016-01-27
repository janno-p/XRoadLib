using System.Collections.Generic;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public abstract class ContainerDefinition<TRuntimeInfo, TElementDefinition>
    {
        public XName Name { get; set; }

        public TRuntimeInfo RuntimeInfo { get; set; }

        public bool HasStrictContentOrder { get; set; }

        public IComparer<TElementDefinition> ContentComparer { get; set; }
    }
}