using System;
using System.Collections.Generic;
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

        public static PropertyDefinition ConvertProperty(PropertyInfo propertyInfo, TypeDefinition ownerDefinition, IProtocol protocol, uint dtoVersion, IDictionary<Type, ITypeMap> partialTypeMaps)
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
                    State = propertyInfo.ExistsInVersion(dtoVersion) ? DefinitionState.Default : DefinitionState.Ignored,
                    TypeMap = GetPropertyTypeMap(customTypeName, propertyInfo.PropertyType.GetElementType(), false, dtoVersion, partialTypeMaps, protocol)
                },
                Order = (elementAttribute?.Order).GetValueOrDefault((arrayAttribute?.Order).GetValueOrDefault()),
                State = propertyInfo.ExistsInVersion(dtoVersion) ? DefinitionState.Default : DefinitionState.Ignored,
                TypeMap = GetPropertyTypeMap(customTypeName, propertyInfo.PropertyType.GetElementType(), propertyInfo.PropertyType.IsArray, dtoVersion, partialTypeMaps, protocol)
            };
        }

        private static ITypeMap GetPropertyTypeMap(string customTypeName, Type runtimeType, bool isArray, uint dtoVersion, IDictionary<Type, ITypeMap> partialTypeMaps, IProtocol protocol)
        {
            return string.IsNullOrWhiteSpace(customTypeName)
                ? protocol.SerializerCache.GetTypeMap(runtimeType, dtoVersion, partialTypeMaps)
                : protocol.SerializerCache.GetTypeMap(XName.Get(customTypeName, NamespaceConstants.XSD), isArray, dtoVersion);
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
                ResponseBinaryMode = (multipartAttribute?.HasMultipartResponse).GetValueOrDefault() ? BinaryMode.SoapAttachment : BinaryMode.Inline
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
    }
}