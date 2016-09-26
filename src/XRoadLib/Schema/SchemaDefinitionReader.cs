using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Styles;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Extracts serialization/definition details from runtime types and methods.
    /// </summary>
    public class SchemaDefinitionReader
    {
        private readonly ISchemaExporter schemaExporter;
        private readonly string producerNamespace;

        /// <summary>
        /// Global settings for protocol instance.
        /// </summary>
        public ProtocolDefinition ProtocolDefinition { get; }

        /// <summary>
        /// Initializes definition builder.
        /// </summary>
        public SchemaDefinitionReader(ISchemaExporter schemaExporter)
        {
            this.schemaExporter = schemaExporter;
            ProtocolDefinition = GetProtocolDefinition();
        }

        /// <summary>
        /// Initializes definition builder.
        /// </summary>
        public SchemaDefinitionReader(string producerNamespace)
            : this((ISchemaExporter)null)
        {
            this.producerNamespace = producerNamespace;
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

            schemaExporter?.ExportTypeDefinition(typeDefinition);

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

            schemaExporter?.ExportTypeDefinition(collectionDefinition);

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
                    qualifiedName = XName.Get(typeName, ProtocolDefinition.ProducerNamespace);

                var collectionDefinition = new CollectionDefinition(type)
                {
                    Name = qualifiedName,
                    ItemDefinition = GetTypeDefinition(type.GetElementType()),
                    IsAnonymous = qualifiedName == null,
                    CanHoldNullValues = true
                };

                schemaExporter?.ExportTypeDefinition(collectionDefinition);

                return collectionDefinition;
            }

            var typeInfo = type.GetTypeInfo();
            var typeAttribute = typeInfo.GetCustomAttribute<XmlTypeAttribute>();
            var isAnonymous = typeAttribute != null && typeAttribute.AnonymousType;

            var normalizedType = Definition.NormalizeType(type);

            if (!isAnonymous)
                qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(typeName ?? normalizedType.Name),
                                          typeAttribute?.Namespace ?? ProtocolDefinition.ProducerNamespace);

            var typeDefinition = new TypeDefinition(normalizedType)
            {
                ContentComparer = PropertyComparer.Instance,
                HasStrictContentOrder = true,
                IsAnonymous = isAnonymous,
                Name = qualifiedName,
                State = DefinitionState.Default,
                CanHoldNullValues = typeInfo.IsClass || normalizedType != type
            };

            schemaExporter?.ExportTypeDefinition(typeDefinition);

            return typeDefinition;
        }

        /// <summary>
        /// Initializes default property definition and applies customizations (if any).
        /// </summary>
        public PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition)
        {
            var propertyDefinition = new PropertyDefinition(propertyInfo, typeDefinition);

            schemaExporter?.ExportPropertyDefinition(propertyDefinition);

            return propertyDefinition;
        }

        /// <summary>
        /// Initializes default request element definition and applies customizations (if any).
        /// </summary>
        public RequestValueDefinition GetRequestValueDefinition(OperationDefinition operationDefinition)
        {
            var requestValueDefinition = new RequestValueDefinition(operationDefinition);

            schemaExporter?.ExportRequestValueDefinition(requestValueDefinition);

            return requestValueDefinition;
        }

        /// <summary>
        /// Initializes default response element definition and applies customizations (if any).
        /// </summary>
        public ResponseValueDefinition GetResponseValueDefinition(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation = null)
        {
            var responseValueDefinition = new ResponseValueDefinition(operationDefinition) { XRoadFaultPresentation = xRoadFaultPresentation ?? XRoadFaultPresentation.Choice };

            schemaExporter?.ExportResponseValueDefinition(responseValueDefinition);

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

            schemaExporter?.ExportOperationDefinition(operationDefinition);

            return operationDefinition;
        }

        /// <summary>
        /// Initializes default fault definition and applies customizations (if any).
        /// </summary>
        public FaultDefinition GetFaultDefinition()
        {
            var faultDefinition = new FaultDefinition { Name = XName.Get("fault", ProtocolDefinition.ProducerNamespace) };

            schemaExporter?.ExportFaultDefinition(faultDefinition);

            return faultDefinition;
        }

        /// <summary>
        /// Get schema location of specified schema namespace.
        /// </summary>
        public string GetSchemaLocation(string namespaceName)
        {
            return schemaExporter?.ExportSchemaLocation(namespaceName) ?? NamespaceConstants.GetSchemaLocation(namespaceName);
        }

        /// <summary>
        /// Customize service description before presentation.
        /// </summary>
        public void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            schemaExporter?.ExportServiceDescription(serviceDescription);
        }

        /// <summary>
        /// Get preferred X-Road namespace prefix for service description.
        /// </summary>
        public string GetXRoadPrefix() => schemaExporter?.XRoadPrefix ?? PrefixConstants.XROAD;

        /// <summary>
        /// Get main namespace which defines X-Road message protocol specifics.
        /// </summary>
        public string GetXRoadNamespace() => schemaExporter?.XRoadNamespace ?? NamespaceConstants.XROAD_V4;

        /// <summary>
        /// Customize X-Road message header elements.
        /// </summary>
        public HeaderDefinition GetXRoadHeaderDefinition()
        {
            var headerDefinition = new HeaderDefinition { MessageName = "RequiredHeaders" };

            headerDefinition.Use(() => new XRoadHeader40(headerDefinition, new DocLiteralStyle()))
                            .WithRequiredHeader(x => x.Client)
                            .WithRequiredHeader(x => x.Client)
                            .WithRequiredHeader(x => x.Service)
                            .WithRequiredHeader(x => x.UserId)
                            .WithRequiredHeader(x => x.Id)
                            .WithRequiredHeader(x => x.Issue)
                            .WithHeaderNamespace(NamespaceConstants.XROAD_V4)
                            .WithHeaderNamespace(NamespaceConstants.XROAD_V4_REPR);

            schemaExporter?.ExportHeaderDefinition(headerDefinition);

            return headerDefinition;
        }

        private ProtocolDefinition GetProtocolDefinition()
        {
            var protocolDefinition = new ProtocolDefinition
            {
                Style = new DocLiteralStyle(),
                ProducerNamespace = producerNamespace
            };

            schemaExporter?.ExportProtocolDefinition(protocolDefinition);

            return protocolDefinition;
        }
    }
}
