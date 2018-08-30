using System;
using System.Xml.Linq;

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