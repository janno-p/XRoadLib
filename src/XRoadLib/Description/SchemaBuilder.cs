using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using XRoadLib.Configuration;
using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib.Description
{
    internal class SchemaBuilder
    {
        private readonly IDictionary<string, Tuple<Type, XmlSchemaType>> schemaTypes = new Dictionary<string, Tuple<Type, XmlSchemaType>>();

        private readonly uint? version;
        private readonly XRoadProtocol protocol;
        private readonly XRoadSchemaBuilder xRoadSchema;
        private readonly XNamespace targetNamespace;
        private readonly ITypeConfiguration typeConfiguration;
        private readonly IOperationConfiguration operationConfiguration;

        internal ISet<string> RequiredImports { get; } = new SortedSet<string>();
        internal IEnumerable<XmlSchemaType> SchemaTypes => schemaTypes.Values.Select(x => x.Item2);

        internal bool this[string typeName] => schemaTypes.ContainsKey(typeName);

        internal SchemaBuilder(XRoadProtocol protocol, XNamespace targetNamespace, IXRoadContractConfiguration configuration, uint? version)
        {
            this.version = version;
            this.protocol = protocol;
            this.targetNamespace = targetNamespace;

            operationConfiguration = configuration?.OperationConfiguration;
            typeConfiguration = configuration?.TypeConfiguration;
            xRoadSchema = new XRoadSchemaBuilder(protocol);
        }

        internal void BuildTypes(Assembly contractAssembly)
        {
            var definedTypes = contractAssembly.GetTypes()
                                               .Where(type => type.IsXRoadSerializable())
                                               .Where(type => !type.IsAnonymous())
                                               .Where(type => !version.HasValue || type.ExistsInVersion(version.Value));

            foreach (var definedType in definedTypes)
            {
                var typeName = definedType.GetProducerTypeName(typeConfiguration, protocol);
                if (typeName.NamespaceName != targetNamespace)
                    continue;

                if (schemaTypes.ContainsKey(typeName.LocalName))
                    throw new Exception($"Multiple type definitions for same name `{typeName}`.");

                var schemaType = new XmlSchemaComplexType { Name = typeName.LocalName, IsAbstract = definedType.IsAbstract, Annotation = xRoadSchema.CreateAnnotationFor(definedType) };

                schemaTypes.Add(typeName.LocalName, Tuple.Create(definedType, (XmlSchemaType)schemaType));
            }

            foreach (var value in schemaTypes.Values)
                AddComplexTypeContent(value.Item1, (XmlSchemaComplexType)value.Item2);
        }

        internal XmlSchemaElement CreateSchemaElement(ParameterInfo parameterInfo)
        {
            var parameterName = parameterInfo.GetParameterName(operationConfiguration);
            var parameterType = parameterInfo.ParameterType;

            if (parameterType.IsArray && parameterType.GetArrayRank() > 1)
                throw new InvalidDataException($"Parameter `{parameterType.Name}` of method `{parameterInfo.Member.DeclaringType?.FullName}.{parameterInfo.Member.Name}` declares multi-dimensional array, which is not supported.");

            return CreateSchemaElement(parameterInfo, parameterName, parameterType);
        }

        internal XmlQualifiedName GetSchemaTypeName(Type type)
        {
            var name = type.GetSystemTypeName() ?? type.GetProducerTypeName(typeConfiguration, protocol);

            if (name.NamespaceName != targetNamespace)
            {
                if (name.NamespaceName != NamespaceConstants.XSD)
                    RequiredImports.Add(name.NamespaceName);
                return new XmlQualifiedName(name.LocalName, name.NamespaceName);
            }

            Tuple<Type, XmlSchemaType> value;
            if (!schemaTypes.TryGetValue(name.LocalName, out value))
                throw new Exception($"Unrecognized type `{name}`.");

            if (value.Item1 != type)
                throw new Exception($"Multiple contract types correspond to same qualified type name `{name}`.");

            return new XmlQualifiedName(name.LocalName, name.NamespaceName);
        }

        internal XmlSchemaSequence CreateFaultSequence(MethodInfo methodInfo)
        {
            if ((operationConfiguration?.HasHiddenXRoadFault(methodInfo)).GetValueOrDefault())
                return null;

            return new XmlSchemaSequence
            {
                Items =
                {
                    new XmlSchemaElement { Name = "faultCode", SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) },
                    new XmlSchemaElement { Name = "faultString", SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) }
                }
            };
        }

        internal XmlSchemaElement CreateSchemaElement(PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.GetPropertyName(typeConfiguration);
            var propertyType = propertyInfo.PropertyType;

            if (propertyType.IsArray && propertyType.GetArrayRank() > 1)
                throw new InvalidDataException($"Property `{propertyInfo.Name}` of type `{propertyInfo.DeclaringType?.FullName}` declares multi-dimensional array, which is not supported.");

            return CreateSchemaElement(propertyInfo, propertyName, propertyType);
        }

        private XmlSchemaElement CreateSchemaElement(ICustomAttributeProvider source, string name, Type type)
        {
            var schemaElement = new XmlSchemaElement { Name = name, Annotation = xRoadSchema.CreateAnnotationFor(source) };

            if (!source.IsRequiredElement())
                schemaElement.MinOccurs = 0;

            var elementAttribute = source.GetSingleAttribute<XmlElementAttribute>();
            if (elementAttribute != null)
                schemaElement.IsNillable = elementAttribute.IsNullable;

            if (type.IsArray && elementAttribute != null)
            {
                schemaElement.MaxOccursString = "unbounded";
                UpdateSchemaElementWithType(schemaElement, source.GetQualifiedElementDataType(), type.GetElementType());
            }
            else if (type.IsArray)
            {
                var arrayAttribute = source.GetSingleAttribute<XmlArrayAttribute>();
                var arrayItemAttribute = source.GetSingleAttribute<XmlArrayItemAttribute>();

                schemaElement.IsNillable = (arrayAttribute?.IsNullable).GetValueOrDefault();

                var itemName = (arrayItemAttribute?.ElementName).GetValueOrDefault("item");
                var itemElement = new XmlSchemaElement { Name = itemName, MinOccurs = 0, MaxOccursString = "unbounded", IsNillable = (arrayItemAttribute?.IsNullable).GetValueOrDefault() };
                UpdateSchemaElementWithType(itemElement, source.GetQualifiedArrayItemDataType(), type.GetElementType());

                AddArrayItemToSchemaElement(schemaElement, itemElement);
            }
            else
                UpdateSchemaElementWithType(schemaElement, source.GetQualifiedElementDataType(), type);

            return schemaElement;
        }

        private void UpdateSchemaElementWithType(XmlSchemaElement schemaElement, XName qualifiedDataType, Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                schemaElement.IsNillable = true;

            var elementType = nullableType ?? type;
            if (elementType == typeof(Stream))
                AddBinaryAttribute(schemaElement);

            if (qualifiedDataType != null)
            {
                schemaElement.SchemaTypeName = new XmlQualifiedName(qualifiedDataType.LocalName.Equals("base64") ? "base64Binary" : qualifiedDataType.LocalName, NamespaceConstants.XSD);
                return;
            }

            if (elementType.IsAnonymous())
            {
                XmlSchemaType schemaType;
                if (elementType.IsEnum)
                {
                    schemaType = new XmlSchemaSimpleType();
                    AddEnumTypeContent(elementType, (XmlSchemaSimpleType)schemaType);
                }
                else
                {
                    schemaType = new XmlSchemaComplexType();
                    AddComplexTypeContent(elementType, (XmlSchemaComplexType)schemaType);
                }

                schemaElement.Annotation = xRoadSchema.CreateAnnotationFor(elementType);
                schemaElement.SchemaType = schemaType;

                return;
            }

            schemaElement.SchemaTypeName = GetSchemaTypeName(elementType);
        }

        private void AddArrayItemToSchemaElement(XmlSchemaElement schemaElement, XmlSchemaElement itemElement)
        {
            if (protocol != XRoadProtocol.Version20)
            {
                schemaElement.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { itemElement } } };
                return;
            }

            RequiredImports.Add(NamespaceConstants.SOAP_ENC);

            var schemaAttribute = new XmlSchemaAttribute { RefName = new XmlQualifiedName("arrayType", NamespaceConstants.SOAP_ENC) };

            if (itemElement.SchemaTypeName != null)
                schemaAttribute.UnhandledAttributes = new[] { xRoadSchema.CreateEncodedArrayTypeAttribute(XName.Get(itemElement.SchemaTypeName.Name, itemElement.SchemaTypeName.Namespace)) };

            var restriction = new XmlSchemaComplexContentRestriction
            {
                BaseTypeName = new XmlQualifiedName("Array", NamespaceConstants.SOAP_ENC),
                Particle = new XmlSchemaSequence { Items = { itemElement } },
                Attributes = { schemaAttribute }
            };

            schemaElement.SchemaType = new XmlSchemaComplexType { ContentModel = new XmlSchemaComplexContent { Content = restriction } };
        }

        private void AddBinaryAttribute(XmlSchemaAnnotated schemaElement)
        {
            if (protocol == XRoadProtocol.Version20)
                return;

            RequiredImports.Add(NamespaceConstants.XMIME);

            schemaElement.UnhandledAttributes = new[] { xRoadSchema.CreateExpectedContentType("application/octet-stream") };
        }

        private void AddEnumTypeContent(Type type, XmlSchemaSimpleType schemaType)
        {
            var restriction = new XmlSchemaSimpleTypeRestriction { BaseTypeName = GetSchemaTypeName(typeof(string)) };

            foreach (var name in Enum.GetNames(type))
            {
                var memberInfo = type.GetMember(name).Single();
                var attribute = memberInfo.GetSingleAttribute<XmlEnumAttribute>();
                restriction.Facets.Add(new XmlSchemaEnumerationFacet { Value = (attribute?.Name).GetValueOrDefault(name) });
            }

            schemaType.Content = restriction;
        }

        private void AddComplexTypeContent(Type type, XmlSchemaComplexType schemaType)
        {
            var properties = type.GetPropertiesSorted(typeConfiguration?.GetPropertyComparer(type) ?? DefaultComparer.Instance, version);

            var contentParticle = new XmlSchemaSequence();

            foreach (var property in properties)
                contentParticle.Items.Add(CreateSchemaElement(property));

            if (type.BaseType != typeof(XRoadSerializable))
            {
                var extension = new XmlSchemaComplexContentExtension { BaseTypeName = GetSchemaTypeName(type.BaseType), Particle = contentParticle };
                var complexContent = new XmlSchemaComplexContent { Content = extension };
                schemaType.ContentModel = complexContent;
            }
            else schemaType.Particle = contentParticle;
        }
    }
}