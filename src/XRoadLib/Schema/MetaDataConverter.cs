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

        private static void AddContentDefinition<T>(ContentDefinition<T> contentDefinition, IProtocol protocol, IDictionary<Type, ITypeMap> partialTypeMaps)
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
                                                           protocol);

            contentDefinition.UseXop = typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType);

            contentDefinition.ArrayItemDefinition = new ArrayItemDefinition
            {
                Name = itemQualifiedName,
                IsNullable = (arrayItemAttribute?.IsNullable).GetValueOrDefault(),
                IsOptional = false,
                TypeMap = GetPropertyTypeMap(customTypeName, contentDefinition.RuntimeType.GetElementType(), false, partialTypeMaps, protocol),
                UseXop = typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType.GetElementType())
            };
        }

        private static PropertyDefinition ConvertProperty(PropertyInfo propertyInfo, TypeDefinition ownerDefinition, IProtocol protocol, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var propertyDefinition = new PropertyDefinition(ownerDefinition) { RuntimeInfo = propertyInfo };

            AddContentDefinition(propertyDefinition, protocol, partialTypeMaps);

            return propertyDefinition;
        }

        private static ITypeMap GetPropertyTypeMap(string customTypeName, Type runtimeType, bool isArray, IDictionary<Type, ITypeMap> partialTypeMaps, IProtocol protocol)
        {
            return string.IsNullOrWhiteSpace(customTypeName)
                ? protocol.SerializerCache.GetTypeMap(runtimeType, partialTypeMaps)
                : protocol.SerializerCache.GetTypeMap(XName.Get(customTypeName, NamespaceConstants.XSD), isArray);
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

        public static ParameterDefinition ConvertParameter(ParameterInfo parameterInfo, OperationDefinition ownerDefinition, IProtocol protocol)
        {
            var parameterDefinition = new ParameterDefinition(ownerDefinition) { RuntimeInfo = parameterInfo };

            AddContentDefinition(parameterDefinition, protocol, null);

            if (parameterDefinition.Name == null)
                parameterDefinition.Name = XName.Get("response");

            return parameterDefinition;
        }

        public static IEnumerable<PropertyDefinition> GetDescriptionProperties(IProtocol protocol, TypeDefinition typeDefinition)
        {
            return typeDefinition.RuntimeInfo
                                 .GetPropertiesSorted(typeDefinition.ContentComparer, p => CreateProperty(protocol, typeDefinition, p, null))
                                 .Where(d => d.State == DefinitionState.Default);
        }

        public static IEnumerable<PropertyDefinition> GetRuntimeProperties(IProtocol protocol, TypeDefinition typeDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return typeDefinition.RuntimeInfo
                                 .GetAllPropertiesSorted(typeDefinition.ContentComparer, p => CreateProperty(protocol, typeDefinition, p, partialTypeMaps))
                                 .Where(d => d.State != DefinitionState.Ignored);
        }

        private static PropertyDefinition CreateProperty(IProtocol protocol, TypeDefinition typeDefinition, PropertyInfo propertyInfo, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var propertyDefinition = ConvertProperty(propertyInfo, typeDefinition, protocol, partialTypeMaps);
            protocol.ExportProperty(propertyDefinition);

            return propertyDefinition;
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