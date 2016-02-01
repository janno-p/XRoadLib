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
    public class SchemaExporter : ISchemaExporter
    {
        public string ProducerNamespace { get; }

        public SchemaExporter(string producerNamespace)
        {
            ProducerNamespace = producerNamespace;
        }

        public virtual void ExportOperationDefinition(OperationDefinition operationDefinition)
        { }

        public virtual void ExportParameterDefinition(ParameterDefinition parameterDefinition)
        { }

        public virtual void ExportPropertyDefinition(PropertyDefinition propertyDefinition)
        { }

        public virtual void ExportTypeDefinition(TypeDefinition typeDefinition)
        { }

        public TypeDefinition GetTypeDefinition(Type type)
        {
            XName qualifiedName = null;

            if (type.IsArray)
            {
                var collectionDefinition = new CollectionDefinition
                {
                    ItemDefinition = GetTypeDefinition(type.GetElementType()),
                    Type = type,
                    IsAnonymous = true,
                    CanHoldNullValues = true
                };

                ExportTypeDefinition(collectionDefinition);

                return collectionDefinition;
            }

            var typeAttribute = type.GetSingleAttribute<XmlTypeAttribute>();
            var isAnonymous = typeAttribute != null && typeAttribute.AnonymousType;

            var normalizedType = Nullable.GetUnderlyingType(type) ?? type;

            if (!isAnonymous)
                qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(normalizedType.Name),
                                          typeAttribute?.Namespace ?? ProducerNamespace);

            var typeDefinition = new TypeDefinition
            {
                ContentComparer = PropertyComparer.Instance,
                HasStrictContentOrder = true,
                IsAnonymous = isAnonymous,
                Name = qualifiedName,
                Type = normalizedType,
                State = DefinitionState.Default,
                CanHoldNullValues = type.IsClass || normalizedType != type
            };

            ExportTypeDefinition(typeDefinition);

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

                var containerName = XName.Get((arrayAttribute?.ElementName).GetValueOrDefault(propertyName), arrayAttribute?.Namespace ?? "");

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
            var propertyDefinition = new PropertyDefinition(typeDefinition)
            {
                PropertyInfo = propertyInfo,
                RuntimeType = propertyInfo.PropertyType
            };

            AddContentDefinition(propertyDefinition, propertyInfo);

            ExportPropertyDefinition(propertyDefinition);

            return propertyDefinition;
        }

        private static ITypeMap GetPropertyTypeMap(string customTypeName, Type runtimeType, bool isArray, IDictionary<Type, ITypeMap> partialTypeMaps, ISerializerCache serializerCache)
        {
            return string.IsNullOrWhiteSpace(customTypeName)
                ? serializerCache.GetTypeMap(runtimeType, partialTypeMaps)
                : serializerCache.GetTypeMap(XName.Get(customTypeName, NamespaceConstants.XSD), isArray);
        }

        public OperationDefinition GetOperationDefinition(MethodInfo methodInfo, XName qualifiedName)
        {
            var multipartAttribute = methodInfo?.GetSingleAttribute<XRoadAttachmentAttribute>();

            var operationDefinition = new OperationDefinition
            {
                Name = qualifiedName,
                HasStrictContentOrder = true,
                MethodInfo = methodInfo,
                RequestBinaryMode = (multipartAttribute?.HasMultipartRequest).GetValueOrDefault() ? BinaryMode.SoapAttachment : BinaryMode.Inline,
                ResponseBinaryMode = (multipartAttribute?.HasMultipartResponse).GetValueOrDefault() ? BinaryMode.SoapAttachment : BinaryMode.Inline,
                State = DefinitionState.Default,
                Version = 1u,
                ContentComparer = ParameterComparer.Instance,
                RequestTypeName = qualifiedName,
                ResponseTypeName = XName.Get($"{qualifiedName.LocalName}Response", qualifiedName.NamespaceName),
                HideXRoadFaultDefinition = false,
                ProhibitRequestPartInResponse = false,
                RequestMessageName = qualifiedName.LocalName,
                ResponseMessageName = $"{qualifiedName.LocalName}Response"
            };

            ExportOperationDefinition(operationDefinition);

            return operationDefinition;
        }

        public ParameterDefinition GetParameterDefinition(ParameterInfo parameterInfo, OperationDefinition operationDefinition)
        {
            var parameterDefinition = new ParameterDefinition(operationDefinition)
            {
                ParameterInfo = parameterInfo,
                RuntimeType = parameterInfo.ParameterType,
                IsResult = parameterInfo.Position < 0
            };

            AddContentDefinition(parameterDefinition, parameterInfo);

            if (parameterDefinition.IsResult && parameterDefinition.Name == null)
                parameterDefinition.Name = XName.Get("value");

            ExportParameterDefinition(parameterDefinition);

            return parameterDefinition;
        }
    }
}
