using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Attributes;
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

        public TypeDefinition GetTypeDefinition(Type type)
        {
            XName qualifiedName = null;

            if (type.IsArray)
            {
                var collectionDefinition = new CollectionDefinition(type)
                {
                    ItemDefinition = GetTypeDefinition(type.GetElementType()),
                    IsAnonymous = true,
                    CanHoldNullValues = true
                };

                SchemaExporter?.ExportTypeDefinition(collectionDefinition);

                return collectionDefinition;
            }

            var typeAttribute = type.GetSingleAttribute<XmlTypeAttribute>();
            var isAnonymous = typeAttribute != null && typeAttribute.AnonymousType;

            var normalizedType = NormalizeType(type);

            if (!isAnonymous)
                qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(normalizedType.Name),
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

        private void AddContentDefinition(ContentDefinition contentDefinition, ICustomAttributeProvider sourceInfo)
        {
            var elementAttribute = sourceInfo.GetSingleAttribute<XmlElementAttribute>();
            var arrayAttribute = sourceInfo.GetSingleAttribute<XmlArrayAttribute>();
            var arrayItemAttribute = sourceInfo.GetSingleAttribute<XmlArrayItemAttribute>();

            if (elementAttribute != null && (arrayAttribute != null || arrayItemAttribute != null))
                throw new Exception($"Property `{contentDefinition.ContainerName}.{contentDefinition.RuntimeName}` should not define XmlElement and XmlArray(Item) attributes at the same time.");

            var propertyName = contentDefinition.RuntimeName;

            XName qualifiedName = null;
            XName itemQualifiedName = null;

            if (contentDefinition.RuntimeType.IsArray)
            {
                if (contentDefinition.RuntimeType.GetArrayRank() > 1)
                    throw new Exception($"Property `{contentDefinition.RuntimeName}` of type `{contentDefinition.ContainerName}` declares multi-dimensional array, which is not supported.");

                var localName = (arrayAttribute?.ElementName).GetValueOrDefault(propertyName);
                var containerName = string.IsNullOrWhiteSpace(localName) ? null : XName.Get(localName, arrayAttribute?.Namespace ?? "");

                if (elementAttribute != null)
                    itemQualifiedName = XName.Get(elementAttribute.ElementName.GetValueOrDefault(propertyName), elementAttribute.Namespace ?? "");
                else if (arrayItemAttribute != null)
                {
                    qualifiedName = containerName;
                    itemQualifiedName = XName.Get(arrayItemAttribute.ElementName.GetValueOrDefault(propertyName), arrayItemAttribute.Namespace ?? "");
                }
                else
                {
                    qualifiedName = containerName;
                    itemQualifiedName = XName.Get("item", "");
                }
            }
            else
            {
                if (arrayAttribute != null || arrayItemAttribute != null)
                    throw new Exception($"Property `{contentDefinition.ContainerName}.{contentDefinition.RuntimeName}` should not define XmlArray(Item) attribute, because it's not array type.");
                var name = (elementAttribute?.ElementName).GetValueOrDefault(propertyName);
                qualifiedName = string.IsNullOrWhiteSpace(name) ? null : XName.Get(name, elementAttribute?.Namespace ?? "");
            }

            var customTypeName = (elementAttribute?.DataType).GetValueOrDefault(arrayItemAttribute?.DataType);

            contentDefinition.Name = qualifiedName;
            contentDefinition.IsNullable = (elementAttribute?.IsNullable).GetValueOrDefault() || (arrayAttribute?.IsNullable).GetValueOrDefault();
            contentDefinition.Order = (elementAttribute?.Order).GetValueOrDefault((arrayAttribute?.Order).GetValueOrDefault());
            contentDefinition.UseXop = typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType);
            contentDefinition.TypeName = customTypeName != null ? XName.Get(customTypeName, NamespaceConstants.XSD) : null;
            contentDefinition.IsOptional = sourceInfo.GetSingleAttribute<XRoadOptionalAttribute>() != null;

            if (!contentDefinition.RuntimeType.IsArray)
                return;

            contentDefinition.ArrayItemDefinition = new ArrayItemDefinition
            {
                Name = itemQualifiedName,
                IsNullable = (arrayItemAttribute?.IsNullable).GetValueOrDefault(),
                IsOptional = false,
                UseXop = typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType.GetElementType()),
                RuntimeType = contentDefinition.RuntimeType.GetElementType()
            };
        }

        public PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition)
        {
            var propertyDefinition = new PropertyDefinition(propertyInfo, typeDefinition)
            {
                RuntimeType = NormalizeType(propertyInfo.PropertyType)
            };

            AddContentDefinition(propertyDefinition, propertyInfo);

            SchemaExporter?.ExportPropertyDefinition(propertyDefinition);

            return propertyDefinition;
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
                HideXRoadFaultDefinition = false,
                ProhibitRequestPartInResponse = false,
                InputMessageName = qualifiedName.LocalName,
                OutputMessageName = $"{qualifiedName.LocalName}Response"
            };

            SchemaExporter?.ExportOperationDefinition(operationDefinition);

            return operationDefinition;
        }

        private static Type NormalizeType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
