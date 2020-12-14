using System;
using System.Collections.Generic;

namespace XRoadLib.Schema
{
    public class TypeDefinition : Definition
    {
        public Type Type { get; }

        public string TargetNamespace { get; }

        public bool CanHoldNullValues { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsSimpleType { get; set; }

        public Type TypeMapType { get; set; }

        public bool HasStrictContentOrder { get; set; }

        public IComparer<PropertyDefinition> ContentComparer { get; set; }

        public bool IsInheritable => !IsAnonymous && !IsSimpleType;

        public bool IsCompositeType => !Type.IsEnum && !Type.IsAbstract;

        public TypeDefinition(Type type, string targetNamespace)
        {
            Documentation = new DocumentationDefinition(type);
            TargetNamespace = targetNamespace;
            Type = type;
        }
    }
}