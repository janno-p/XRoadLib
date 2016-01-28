using System;
using System.Reflection;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition<TRuntimeInfo> : Definition<TRuntimeInfo> where TRuntimeInfo : ICustomAttributeProvider
    {
        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        public ArrayItemDefinition ArrayItemDefinition { get; set; }

        public int Order { get; set; }

        public ITypeMap TypeMap { get; set; }

        public abstract string ContainerName { get; }

        public abstract string RuntimeName { get; }

        public abstract Type RuntimeType { get; }
    }
}