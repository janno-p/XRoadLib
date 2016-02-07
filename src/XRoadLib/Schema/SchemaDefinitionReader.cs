using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    public class SchemaDefinitionReader
    {
        public string ProducerNamespace { get; }

        public ISchemaExporter SchemaExporter { get; }

        public SchemaDefinitionReader(string producerNamespace, ISchemaExporter schemaExporter = null)
        {
            ProducerNamespace = producerNamespace;
            SchemaExporter = schemaExporter;
        }

        public TypeDefinition GetSimpleTypeDefinition<T>(string typeName)
        {
            var typeDefinition = new TypeDefinition(typeof(T))
            {
                Name = XName.Get(typeName, NamespaceConstants.XSD),
                IsSimpleType = true
            };

            SchemaExporter?.ExportTypeDefinition(typeDefinition);

            return typeDefinition;
        }

        public CollectionDefinition GetCollectionDefinition(TypeDefinition typeDefinition)
        {
            var collectionDefinition = new CollectionDefinition(typeDefinition.Type.MakeArrayType())
            {
                ItemDefinition = typeDefinition,
                CanHoldNullValues = true,
                IsAnonymous = true
            };

            SchemaExporter?.ExportTypeDefinition(collectionDefinition);

            return collectionDefinition;
        }

        public TypeDefinition GetTypeDefinition(Type type, string typeName = null)
        {
            XName qualifiedName = null;

            if (type.IsArray)
            {
                if (!string.IsNullOrWhiteSpace(typeName))
                    qualifiedName = XName.Get(typeName, ProducerNamespace);

                var collectionDefinition = new CollectionDefinition(type)
                {
                    Name = qualifiedName,
                    ItemDefinition = GetTypeDefinition(type.GetElementType()),
                    IsAnonymous = qualifiedName == null,
                    CanHoldNullValues = true
                };

                SchemaExporter?.ExportTypeDefinition(collectionDefinition);

                return collectionDefinition;
            }

            var typeAttribute = type.GetSingleAttribute<XmlTypeAttribute>();
            var isAnonymous = typeAttribute != null && typeAttribute.AnonymousType;

            var normalizedType = Definition.NormalizeType(type);

            if (!isAnonymous)
                qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(typeName ?? normalizedType.Name),
                                          typeAttribute?.Namespace ?? ProducerNamespace);

            var typeDefinition = new TypeDefinition(normalizedType)
            {
                ContentComparer = PropertyComparer.Instance,
                HasStrictContentOrder = true,
                IsAnonymous = isAnonymous,
                Name = qualifiedName,
                State = DefinitionState.Default,
                CanHoldNullValues = type.IsClass || normalizedType != type
            };

            SchemaExporter?.ExportTypeDefinition(typeDefinition);

            return typeDefinition;
        }

        public PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition)
        {
            var propertyDefinition = new PropertyDefinition(propertyInfo, typeDefinition);

            SchemaExporter?.ExportPropertyDefinition(propertyDefinition);

            return propertyDefinition;
        }

        public ResponseValueDefinition GetResponseValueDefinition(OperationDefinition operationDefinition)
        {
            var responseValueDefinition = new ResponseValueDefinition(operationDefinition);

            SchemaExporter?.ExportResponseValueDefinition(responseValueDefinition);

            return responseValueDefinition;
        }

        private static ITypeMap GetPropertyTypeMap(string customTypeName, Type runtimeType, bool isArray, IDictionary<Type, ITypeMap> partialTypeMaps, ISerializerCache serializerCache)
        {
            return string.IsNullOrWhiteSpace(customTypeName)
                ? serializerCache.GetTypeMap(runtimeType, partialTypeMaps)
                : serializerCache.GetTypeMap(XName.Get(customTypeName, NamespaceConstants.XSD), isArray);
        }

        public OperationDefinition GetOperationDefinition(MethodInfo methodInfo, XName qualifiedName, uint? version)
        {
            var serviceAttribute = methodInfo.GetServices().SingleOrDefault(x => x.Name == qualifiedName.LocalName);

            var operationDefinition = new OperationDefinition(methodInfo)
            {
                Name = qualifiedName,
                IsAbstract = (serviceAttribute?.IsAbstract).GetValueOrDefault(),
                InputBinaryMode = BinaryMode.Xml,
                OutputBinaryMode = BinaryMode.Xml,
                State = (serviceAttribute?.IsHidden).GetValueOrDefault() ? DefinitionState.Hidden : DefinitionState.Default,
                Version = version.GetValueOrDefault(serviceAttribute?.AddedInVersion ?? 1u),
                ProhibitRequestPartInResponse = false,
                InputMessageName = qualifiedName.LocalName,
                OutputMessageName = $"{qualifiedName.LocalName}Response"
            };

            SchemaExporter?.ExportOperationDefinition(operationDefinition);

            return operationDefinition;
        }
    }
}
