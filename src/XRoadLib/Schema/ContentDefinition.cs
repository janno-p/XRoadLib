using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition<TRuntimeInfo>
    {
        public XName Name { get; set; }

        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public PropertyDefinition ItemProperty { get; set; }

        public TRuntimeInfo RuntimeInfo { get; set; }

        public int Order { get; set; }
    }
}