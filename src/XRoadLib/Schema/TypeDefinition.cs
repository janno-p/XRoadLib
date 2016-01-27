using System;

namespace XRoadLib.Schema
{
    public class TypeDefinition : ContainerDefinition<Type, PropertyDefinition>
    {
        public bool IsAnonymous { get; set; }
    }
}