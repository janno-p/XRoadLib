namespace XRoadLib.Schema
{
    public abstract class ContentDefinition<TRuntimeInfo> : Definition<TRuntimeInfo>
    {
        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public PropertyDefinition ItemDefinition { get; set; }

        public TypeDefinition TypeDefinition { get; set; }

        public int Order { get; set; }
    }
}