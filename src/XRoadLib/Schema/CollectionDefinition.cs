using System;
using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Schema
{
    public class CollectionDefinition : TypeDefinition
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public TypeDefinition ItemDefinition { get; set; }

        public CollectionDefinition(Type type, string targetNamespace)
            : base(type, targetNamespace)
        { }
    }
}