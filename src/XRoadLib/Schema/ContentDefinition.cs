using System;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition : Definition
    {
        public ParticleDefinition Particle { get; }

        public bool IgnoreExplicitType { get; set; }

        public bool MergeContent { get; set; }

        public bool IsNullable { get; set; }

        public virtual bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        public int Order { get; set; } = -1;

        public XName TypeName { get; set; }

        public Type RuntimeType { get; set; }

        public virtual XName SerializedName => Name;

        protected ContentDefinition(ParticleDefinition particle)
        {
            Particle = particle;
        }

        public static ContentDefinition FromType(ParticleDefinition particle, ICustomAttributeProvider customAttributeProvider, Type runtimeType, string runtimeName)
        {
            if (runtimeType.IsArray)
                return new ArrayContentDefiniton(particle, customAttributeProvider, runtimeType, runtimeName);
            
            return new SingularContentDefinition(particle, customAttributeProvider, runtimeType, runtimeName);
        }
    }
}