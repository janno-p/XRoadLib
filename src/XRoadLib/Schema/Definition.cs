using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public abstract class Definition<TRuntimeInfo>
        where TRuntimeInfo : ICustomAttributeProvider
    {
        public XName Name { get; set; }

        public TRuntimeInfo RuntimeInfo { get; set; }

        public DefinitionState State { get; set; }
    }
}