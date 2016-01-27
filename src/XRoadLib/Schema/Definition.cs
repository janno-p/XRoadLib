using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public abstract class Definition<TRuntimeInfo>
    {
        public XName Name { get; set; }

        public TRuntimeInfo RuntimeInfo { get; set; }

        public DefinitionState State { get; set; }
    }
}