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

            if (!isAnonymous)
                qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(type.Name),
                                          typeAttribute?.Namespace ?? protocol.ProducerNamespace);

            return new TypeDefinition
            {
                ContentComparer = PropertyComparer.Instance,
                HasStrictContentOrder = true,
                IsAnonymous = isAnonymous,
                Name = qualifiedName,
                RuntimeInfo = type,
                State = DefinitionState.Default
            };
        }

        public static PropertyDefinition ConvertProperty(PropertyInfo propertyInfo, TypeDefinition ownerDefinition, IProtocol protocol, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var elementAttribute = propertyInfo.GetSingleAttribute<XmlElementAttribute>();
            var arrayAttribute = propertyInfo.GetSingleAttribute<XmlArrayAttribute>();
            var arrayItemAttribute = propertyInfo.GetSingleAttribute<XmlArrayItemAttribute>();

            if (elementAttribute != null && (arrayAttribute != null || arrayItemAttribute != null))
                throw new Exception($"Property `{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}` should not define XmlElement and XmlArray(Item) attributes at the same time.");

            var startIndex = propertyInfo.Name.LastIndexOf('.');
            var propertyName = startIndex >= 0 ? propertyInfo.Name.Substring(startIndex + 1) : propertyInfo.Name;

            XName qualifiedName = null;
            XName itemQualifiedName = null;

            if (propertyInfo.PropertyType.IsArray)
            {
                if (propertyInfo.PropertyType.IsArray && propertyInfo.PropertyType.GetArrayRank() > 1)
                    throw new Exception($"Property `{propertyInfo.Name}` of type `{propertyInfo.DeclaringType?.FullName}` declares multi-dimensional array, which is not supported.");

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
                    throw new Exception($"Property `{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}` should not define XmlArray(Item) attribute, because it's not array type.");
                qualifiedName = XName.Get((elementAttribute?.ElementName).GetValueOrDefault(propertyName), elementAttribute?.Namespace ?? "");
            }

            var customTypeName = (elementAttribute?.DataType).GetValueOrDefault(arrayItemAttribute?.DataType);

            return new PropertyDefinition(ownerDefinition)
            {
                Name = qualifiedName,
                RuntimeInfo = propertyInfo,
                IsNullable = (elementAttribute?.IsNullable).GetValueOrDefault() || (arrayAttribute?.IsNullable).GetValueOrDefault(),
                IsOptional = false,
                ItemDefinition = new PropertyDefinition(ownerDefinition)
                {
                    Name = itemQualifiedName,
                    RuntimeInfo = propertyInfo,
                    IsNullable = (arrayItemAttribute?.IsNullable).GetValueOrDefault(),
                    IsOptional = true,
                    State = DefinitionState.Default,
                    TypeMap = partialTypeMaps != null ? GetPropertyTypeMap(customTypeName, propertyInfo.PropertyType.GetElementType(), false, partialTypeMaps, protocol) : null,
                    UseXop = typeof(Stream).IsAssignableFrom(propertyInfo.PropertyType.GetElementType())
                },
                Order = (elementAttribute?.Order).GetValueOrDefault((arrayAttribute?.Order).GetValueOrDefault()),
                State = DefinitionState.Default,
                TypeMap = partialTypeMaps != null ? GetPropertyTypeMap(customTypeName, propertyInfo.PropertyType.GetElementType(), propertyInfo.PropertyType.IsArray, partialTypeMaps, protocol) : null,
                UseXop = typeof(Stream).IsAssignableFrom(propertyInfo.PropertyType)
            };
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

        public static ParameterDefinition ConvertParameter(ParameterInfo parameterInfo, OperationDefinition ownerDefinition)
        {
            return new ParameterDefinition(ownerDefinition)
            {
                Name = parameterInfo.GetElementName() ?? XName.Get(parameterInfo.Name.GetValueOrDefault("response")),
                RuntimeInfo = parameterInfo
            };
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