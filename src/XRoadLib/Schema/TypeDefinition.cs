using System;
using System.Xml.Linq;

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

        public bool IsInheritable { get { return !IsAnonymous && !IsSimpleType; } }

        public CollectionDefinition CreateCollectionDefinition()
        {
            return new CollectionDefinition
            {
                ItemDefinition = this,
                CanHoldNullValues = true,
                Type = Type.MakeArrayType(),
                IsAnonymous = true
            };
        }

        public static TypeDefinition SimpleTypeDefinition<T>(string typeName)
        {
            return new TypeDefinition
            {
                Name = XName.Get(typeName, NamespaceConstants.XSD),
                Type = typeof(T),
                IsSimpleType = true
            };
        }
    }
}