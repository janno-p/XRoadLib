using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition<TRuntimeInfo> : Definition<TRuntimeInfo>
    {
        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        public PropertyDefinition ItemDefinition { get; set; }

        public int Order { get; set; }

        public ITypeMap TypeMap { get; set; }
    }
}