using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Extracts serialization/definition details from runtime types and methods.
    /// </summary>
    public class SchemaDefinitionReader
    {
        /// <summary>
        /// Service description target namespace.
        /// </summary>
        public string ProducerNamespace { get; }

        /// <summary>
        /// Customizations provider.
        /// </summary>
        public ISchemaExporter SchemaExporter { get; }

        /// <summary>
        /// Initializes definition builder.
        /// </summary>
        public SchemaDefinitionReader(string producerNamespace, ISchemaExporter schemaExporter = null)
        {
            ProducerNamespace = producerNamespace;
            SchemaExporter = schemaExporter;
        }

        /// <summary>
        /// Initializes default simple type definition and applies customizations (if any).
        /// </summary>
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

        /// <summary>
        /// Initializes default collection type definition and applies customizations (if any).
        /// </summary>
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

        /// <summary>
        /// Initializes default type definition and applies customizations (if any).
        /// </summary>
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

            var typeInfo = type.GetTypeInfo();
            var typeAttribute = typeInfo.GetCustomAttribute<XmlTypeAttribute>();
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
                CanHoldNullValues = typeInfo.IsClass || normalizedType != type
            };

            SchemaExporter?.ExportTypeDefinition(typeDefinition);

            return typeDefinition;
        }

        /// <summary>
        /// Initializes default property definition and applies customizations (if any).
        /// </summary>
        public PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition)
        {
            var propertyDefinition = new PropertyDefinition(propertyInfo, typeDefinition);

            SchemaExporter?.ExportPropertyDefinition(propertyDefinition);

            return propertyDefinition;
        }

        /// <summary>
        /// Initializes default request element definition and applies customizations (if any).
        /// </summary>
        public RequestValueDefinition GetRequestValueDefinition(OperationDefinition operationDefinition)
        {
            var requestValueDefinition = new RequestValueDefinition(operationDefinition);

            SchemaExporter?.ExportRequestValueDefinition(requestValueDefinition);

            return requestValueDefinition;
        }

        /// <summary>
        /// Initializes default response element definition and applies customizations (if any).
        /// </summary>
        public ResponseValueDefinition GetResponseValueDefinition(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation = null)
        {
            var responseValueDefinition = new ResponseValueDefinition(operationDefinition) { XRoadFaultPresentation = xRoadFaultPresentation ?? XRoadFaultPresentation.Choice };

            SchemaExporter?.ExportResponseValueDefinition(responseValueDefinition);

            return responseValueDefinition;
        }

        private static ITypeMap GetPropertyTypeMap(string customTypeName, Type runtimeType, bool isArray, IDictionary<Type, ITypeMap> partialTypeMaps, ISerializerCache serializerCache)
        {
            return string.IsNullOrWhiteSpace(customTypeName)
                ? serializerCache.GetTypeMap(runtimeType, partialTypeMaps)
                : serializerCache.GetTypeMap(XName.Get(customTypeName, NamespaceConstants.XSD), isArray);
        }

        /// <summary>
        /// Initializes default opeartion definition and applies customizations (if any).
        /// </summary>
        public OperationDefinition GetOperationDefinition(MethodInfo methodInfo, XName qualifiedName, uint? version)
        {
            var operationDefinition = new OperationDefinition(qualifiedName, version, methodInfo);

            SchemaExporter?.ExportOperationDefinition(operationDefinition);

            return operationDefinition;
        }

        /// <summary>
        /// Initializes default fault definition and applies customizations (if any).
        /// </summary>
        public FaultDefinition GetFaultDefinition()
        {
            var faultDefinition = new FaultDefinition { Name = XName.Get("fault", ProducerNamespace) };

            SchemaExporter?.ExportFaultDefinition(faultDefinition);

            return faultDefinition;
        }

        /// <summary>
        /// Get schema location of specified schema namespace.
        /// </summary>
        public string GetSchemaLocation(string namespaceName)
        {
            return SchemaExporter?.ExportSchemaLocation(namespaceName) ?? NamespaceConstants.GetSchemaLocation(namespaceName);
        }

        /// <summary>
        /// Customize service description before presentation.
        /// </summary>
        public void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            SchemaExporter?.ExportServiceDescription(serviceDescription);
        }
    }
}
