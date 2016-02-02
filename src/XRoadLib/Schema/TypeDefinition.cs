using System;

namespace XRoadLib.Schema
{
    public class TypeDefinition : ContainerDefinition<PropertyDefinition>
    {
        public Type Type { get; set; }

        public bool CanHoldNullValues { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsSimpleType { get; set; }

        public Type TypeMapType { get; set; }

        public bool IsInheritable => !IsAnonymous && !IsSimpleType;
    }
}