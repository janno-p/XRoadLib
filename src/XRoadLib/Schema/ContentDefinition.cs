namespace XRoadLib.Schema
{
    public abstract class ContentDefinition<TRuntimeInfo> : Definition<TRuntimeInfo>
    {
        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public PropertyDefinition ItemProperty { get; set; }

        public int Order { get; set; }
    }
}