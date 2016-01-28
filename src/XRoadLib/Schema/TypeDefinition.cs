using System;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    public class TypeDefinition : ContainerDefinition<Type, PropertyDefinition>
    {
        public bool IsAnonymous { get; set; }

        public ITypeMap TypeMap { get; set; }
    }
}