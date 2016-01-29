using System;
using System.Xml.Linq;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    public class TypeDefinition : ContainerDefinition<Type, PropertyDefinition>
    {
        public bool CanHoldNullValues { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsSimpleType { get; set; }

        public ITypeMap TypeMap { get; set; }

        public CollectionDefinition CreateCollectionDefinition()
        {
            return new CollectionDefinition
            {
                ItemDefinition = this,
                CanHoldNullValues = true,
                RuntimeInfo = RuntimeInfo.MakeArrayType(),
                IsAnonymous = true
            };
        }

        public static TypeDefinition SimpleTypeDefinition<T>(string typeName)
        {
            return new TypeDefinition
            {
                Name = XName.Get(typeName, NamespaceConstants.XSD),
                RuntimeInfo = typeof(T),
                IsSimpleType = true
            };
        }
    }
}