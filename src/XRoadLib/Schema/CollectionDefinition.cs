using System;

namespace XRoadLib.Schema
{
    public class CollectionDefinition : TypeDefinition
    {
        public TypeDefinition ItemDefinition { get; set; }

        public CollectionDefinition(Type type, string targetNamespace)
            : base(type, targetNamespace)
        { }
    }
}