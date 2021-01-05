using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace XRoadLib.Schema
{
    public class TypeDefinition : Definition
    {
        public Type Type { get; }

        public string TargetNamespace { get; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public bool CanHoldNullValues { get; set; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public bool IsAbstract { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsSimpleType { get; set; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Type TypeMapType { get; set; }

        public bool HasStrictContentOrder { get; set; }

        public IComparer<PropertyDefinition> ContentComparer { get; set; }

        public bool IsInheritable => !IsAnonymous && !IsSimpleType;

        public bool IsCompositeType => !Type.GetTypeInfo().IsEnum && !Type.GetTypeInfo().IsAbstract;

        public TypeDefinition(Type type, string targetNamespace)
        {
            Documentation = new DocumentationDefinition(type.GetTypeInfo());
            TargetNamespace = targetNamespace;
            Type = type;
        }
    }
}