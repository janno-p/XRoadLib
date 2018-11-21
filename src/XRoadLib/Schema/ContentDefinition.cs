using System;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition : Definition
    {
        public ParticleDefinition Particle { get; }

        public bool IgnoreExplicitType { get; set; }

        public bool MergeContent { get; set; }

        public bool IsNullable { get; set; }

        public EmptyTagHandlingMode? EmptyTagHandlingMode { get; set; }

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

        public static ContentDefinition FromType(ParticleDefinition particle, ICustomAttributeProvider customAttributeProvider, Type runtimeType, string runtimeName, string targetNamespace, bool defaultQualifiedElement)
        {
            if (customAttributeProvider == null)
                return new EmptyContentDefinition(particle, runtimeName);

            if (runtimeType.IsArray)
                return new ArrayContentDefiniton(particle, customAttributeProvider, runtimeType, runtimeName, targetNamespace, defaultQualifiedElement);
            
            return new SingularContentDefinition(particle, customAttributeProvider, runtimeType, runtimeName, targetNamespace, defaultQualifiedElement);
        }

        public static string GetQualifiedNamespace(string elementNamespace, XmlSchemaForm? elementForm, string targetNamespace, bool defaultQualifiedElement)
        {
            switch (elementForm.GetValueOrDefault())
            {
                case XmlSchemaForm.Qualified:
                    return string.IsNullOrEmpty(elementNamespace) ? targetNamespace : elementNamespace;

                case XmlSchemaForm.Unqualified:
                    return string.Empty;

                default:
                    return string.IsNullOrEmpty(elementNamespace) ? (defaultQualifiedElement ? targetNamespace : string.Empty) : elementNamespace;
            }
        }
    }
}