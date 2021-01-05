using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition : Definition
    {
        public ParticleDefinition Particle { get; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public bool IgnoreExplicitType { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public bool MergeContent { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public bool IsNullable { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public EmptyTagHandlingMode? EmptyTagHandlingMode { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public virtual bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
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
                return new EmptyContentDefinition(particle);

            if (runtimeType.IsArray)
                return new ArrayContentDefiniton(particle, customAttributeProvider, runtimeType, runtimeName, targetNamespace, defaultQualifiedElement);
            
            return new SingularContentDefinition(particle, customAttributeProvider, runtimeType, runtimeName, targetNamespace, defaultQualifiedElement);
        }

        public static string GetQualifiedNamespace(string elementNamespace, XmlSchemaForm? elementForm, string targetNamespace, bool defaultQualifiedElement) => elementForm.GetValueOrDefault() switch
        {
            XmlSchemaForm.Qualified => string.IsNullOrEmpty(elementNamespace) ? targetNamespace : elementNamespace,
            XmlSchemaForm.Unqualified => string.Empty,
            _ => string.IsNullOrEmpty(elementNamespace) ? defaultQualifiedElement ? targetNamespace : string.Empty : elementNamespace
        };
    }
}