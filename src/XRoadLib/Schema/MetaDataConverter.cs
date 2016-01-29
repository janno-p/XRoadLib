using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    public static class MetaDataConverter
    {
        public static TypeDefinition ConvertType(Type type, IProtocol protocol)
        {
            XName qualifiedName = null;

            var typeAttribute = type.GetSingleAttribute<XmlTypeAttribute>();
            var isAnonymous = typeAttribute != null && typeAttribute.AnonymousType;

            var normalizedType = Nullable.GetUnderlyingType(type) ?? type;

            if (!isAnonymous)
                qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(normalizedType.Name),
                                          typeAttribute?.Namespace ?? protocol.ProducerNamespace);

            return new TypeDefinition
            {
                ContentComparer = PropertyComparer.Instance,
                HasStrictContentOrder = true,
                IsAnonymous = isAnonymous,
                Name = qualifiedName,
                RuntimeInfo = normalizedType,
                State = DefinitionState.Default
            };
        }

        private static void AddContentDefinition<T>(ContentDefinition<T> contentDefinition, ISerializerCache serializerCache, IDictionary<Type, ITypeMap> partialTypeMaps)
            where T : ICustomAttributeProvider
        {
            var sourceInfo = contentDefinition.RuntimeInfo;

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
                qualifiedName = name != null ? XName.Get(name, elementAttribute?.Namespace ?? "") : null;
            }

            var customTypeName = (elementAttribute?.DataType).GetValueOrDefault(arrayItemAttribute?.DataType);

            contentDefinition.Name = qualifiedName;
            contentDefinition.IsNullable = (elementAttribute?.IsNullable).GetValueOrDefault() || (arrayAttribute?.IsNullable).GetValueOrDefault();
            contentDefinition.Order = (elementAttribute?.Order).GetValueOrDefault((arrayAttribute?.Order).GetValueOrDefault());

            contentDefinition.TypeMap = GetPropertyTypeMap(customTypeName,
                                                           contentDefinition.RuntimeType.IsArray ? contentDefinition.RuntimeType.GetElementType()
                                                                                                 : contentDefinition.RuntimeType,
                                                           contentDefinition.RuntimeType.IsArray,
                                                           partialTypeMaps,
                                                           serializerCache);

            contentDefinition.UseXop = typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType);

            contentDefinition.ArrayItemDefinition = new ArrayItemDefinition
            {
                Name = itemQualifiedName,
                IsNullable = (arrayItemAttribute?.IsNullable).GetValueOrDefault(),
                IsOptional = false,
                TypeMap = GetPropertyTypeMap(customTypeName, contentDefinition.RuntimeType.GetElementType(), false, partialTypeMaps, serializerCache),
                UseXop = typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType.GetElementType())
            };
        }

        private static PropertyDefinition ConvertProperty(PropertyInfo propertyInfo, TypeDefinition ownerDefinition, ISerializerCache serializerCache, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var propertyDefinition = new PropertyDefinition(ownerDefinition) { RuntimeInfo = propertyInfo };

            AddContentDefinition(propertyDefinition, serializerCache, partialTypeMaps);

            serializerCache.Protocol.ExportProperty(propertyDefinition);

            return propertyDefinition;
        }

        private static ITypeMap GetPropertyTypeMap(string customTypeName, Type runtimeType, bool isArray, IDictionary<Type, ITypeMap> partialTypeMaps, ISerializerCache serializerCache)
        {
            return string.IsNullOrWhiteSpace(customTypeName)
                ? serializerCache.GetTypeMap(runtimeType, partialTypeMaps)
                : serializerCache.GetTypeMap(XName.Get(customTypeName, NamespaceConstants.XSD), isArray);
        }

        public static OperationDefinition ConvertOperation(MethodInfo methodInfo, XName qualifiedName)
        {
            var multipartAttribute = methodInfo.GetSingleAttribute<XRoadAttachmentAttribute>();

            return new OperationDefinition
            {
                Name = qualifiedName,
                HasStrictContentOrder = true,
                RuntimeInfo = methodInfo,
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
        }

        public static ParameterDefinition ConvertParameter(ParameterInfo parameterInfo, OperationDefinition ownerDefinition, ISerializerCache serializerCache)
        {
            var parameterDefinition = new ParameterDefinition(ownerDefinition) { RuntimeInfo = parameterInfo };

            AddContentDefinition(parameterDefinition, serializerCache, null);

            if (parameterDefinition.Name == null)
                parameterDefinition.Name = XName.Get("response");

            serializerCache.Protocol.ExportParameter(parameterDefinition);

            return parameterDefinition;
        }

        public static IEnumerable<PropertyDefinition> GetDescriptionProperties(ISerializerCache serializerCache, TypeDefinition typeDefinition)
        {
            return typeDefinition.RuntimeInfo
                                 .GetPropertiesSorted(typeDefinition.ContentComparer, serializerCache.Version, p => ConvertProperty(p, typeDefinition, serializerCache, null))
                                 .Where(d => d.State == DefinitionState.Default);
        }

        public static IEnumerable<PropertyDefinition> GetRuntimeProperties(ISerializerCache serializerCache, TypeDefinition typeDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return typeDefinition.RuntimeInfo
                                 .GetAllPropertiesSorted(typeDefinition.ContentComparer, serializerCache.Version, p => ConvertProperty(p, typeDefinition, serializerCache, partialTypeMaps))
                                 .Where(d => d.State != DefinitionState.Ignored);
        }

        private static string GetOperationNameFromMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType == null)
                throw new ArgumentException("Declaring type is missing.", nameof(methodInfo));

            if (methodInfo.DeclaringType.Name.StartsWith("I", StringComparison.InvariantCulture) && methodInfo.DeclaringType.Name.Length > 1 && char.IsUpper(methodInfo.DeclaringType.Name[1]))
                return methodInfo.DeclaringType.Name.Substring(1);

            return methodInfo.DeclaringType.Name;
        }
    }
}