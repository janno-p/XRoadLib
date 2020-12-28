using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Styles;

namespace XRoadLib.Schema
{
    public class DefaultSchemaProvider : ISchemaProvider
    {
        private readonly Assembly _contractAssembly;
        
        /// <summary>
        /// Preferred X-Road namespace prefix of the message protocol version.
        /// </summary>
        public virtual string XRoadPrefix => PrefixConstants.XRoad;

        /// <summary>
        /// X-Road specification namespace of the message protocol version.
        /// </summary>
        public virtual string XRoadNamespace => NamespaceConstants.XRoad;

        /// <summary>
        /// Defines list of supported DTO versions (for DTO based versioning).
        /// </summary>
        public ISet<uint> SupportedVersions { get; } = new HashSet<uint>();

        /// <summary>
        /// Define list of content filters of X-Road message elements.
        /// </summary>
        public ISet<string> EnabledFilters { get; } = new HashSet<string>();

        /// <summary>
        /// Producer namespace of exported X-Road schema.
        /// </summary>
        public virtual string ProducerNamespace { get; }

        /// <summary>
        /// Initializes default schema provider with producer namespace.
        /// </summary>
        public DefaultSchemaProvider(string producerNamespace, Assembly contractAssembly)
        {
            ProducerNamespace = producerNamespace;

            _contractAssembly = contractAssembly;
        }
        
        /// <summary>
        /// Initializes default type definition and applies customizations (if any).
        /// </summary>
        public virtual TypeDefinition GetTypeDefinition(Type type, string typeName = null)
        {
            XName qualifiedName = null;

            if (type.IsArray)
            {
                if (!string.IsNullOrWhiteSpace(typeName))
                    qualifiedName = XName.Get(typeName, ProducerNamespace);

                var collectionDefinition = new CollectionDefinition(type, ProducerNamespace)
                {
                    Name = qualifiedName,
                    ItemDefinition = GetTypeDefinition(type.GetElementType()),
                    IsAnonymous = qualifiedName == null,
                    CanHoldNullValues = true
                };

                return collectionDefinition;
            }

            var typeAttribute = type.GetCustomAttribute<XmlTypeAttribute>();
            var isAnonymous = typeAttribute != null && typeAttribute.AnonymousType;

            var normalizedType = type.NormalizeType();
            var targetNamespace = typeAttribute?.Namespace ?? ProducerNamespace;

            if (!isAnonymous)
                qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(typeName ?? normalizedType.Name), targetNamespace);

            var typeDefinition = new TypeDefinition(normalizedType, targetNamespace)
            {
                ContentComparer = PropertyComparer.Instance,
                HasStrictContentOrder = true,
                IsAnonymous = isAnonymous,
                Name = qualifiedName,
                State = DefinitionState.Default,
                CanHoldNullValues = type.IsClass || normalizedType != type
            };

            return typeDefinition;
        }
        
        /// <summary>
        /// Initializes default simple type definition and applies customizations (if any).
        /// </summary>
        public virtual TypeDefinition GetSimpleTypeDefinition<T>(string typeName)
        {
            var isType = !string.IsNullOrEmpty(typeName);

            var typeDefinition = new TypeDefinition(typeof(T), isType ? NamespaceConstants.Xsd : "")
            {
                Name = isType ? XName.Get(typeName, NamespaceConstants.Xsd) : null,
                IsSimpleType = true
            };

            return typeDefinition;
        }

        /// <summary>
        /// Initializes default collection type definition and applies customizations (if any).
        /// </summary>
        public virtual CollectionDefinition GetCollectionDefinition(TypeDefinition typeDefinition) =>
            new CollectionDefinition(typeDefinition.Type.MakeArrayType(), typeDefinition.TargetNamespace)
            {
                ItemDefinition = typeDefinition,
                CanHoldNullValues = true,
                IsAnonymous = true
            };
        
        /// <summary>
        /// Initializes default property definition and applies customizations (if any).
        /// </summary>
        public virtual PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition) =>
            new PropertyDefinition(propertyInfo, typeDefinition, IsQualifiedElementDefault);
        
        /// <summary>
        /// Initializes default operation definition and applies customizations (if any).
        /// </summary>
        public virtual OperationDefinition GetOperationDefinition(Type operationType, XName qualifiedName, uint? version)
        {
            var attribute = operationType.GetOperations().SingleOrDefault(x => qualifiedName.LocalName == x.GetNameOrDefault(operationType));
            
            var extensionSchemaProvider = attribute?.SchemaProvider;

            var operationDefinition = extensionSchemaProvider?.GetOperationDefinition(operationType, qualifiedName, version)
                                      ?? new OperationDefinition(qualifiedName, version, operationType, attribute);

            operationDefinition.ExtensionSchemaProvider = extensionSchemaProvider;
            
            return operationDefinition;
        }

        /// <summary>
        /// Initializes default request element definition and applies customizations (if any).
        /// </summary>
        public virtual RequestDefinition GetRequestDefinition(OperationDefinition operationDefinition) =>
            operationDefinition.ExtensionSchemaProvider?.GetRequestDefinition(operationDefinition)
            ?? new RequestDefinition(operationDefinition, IsQualifiedElementDefault);

        /// <summary>
        /// Initializes default response element definition and applies customizations (if any).
        /// </summary>
        public virtual ResponseDefinition GetResponseDefinition(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation = null) =>
            operationDefinition.ExtensionSchemaProvider?.GetResponseDefinition(operationDefinition, xRoadFaultPresentation)
            ?? new ResponseDefinition(operationDefinition, IsQualifiedElementDefault)
            {
                XRoadFaultPresentation = xRoadFaultPresentation ?? XRoadFaultPresentation.Choice
            };

        /// <summary>
        /// Customize X-Road message header elements.
        /// </summary>
        public virtual IHeaderDefinition GetXRoadHeaderDefinition() =>
            HeaderDefinition.Default;

        /// <summary>
        /// Customize X-Road protocol global settings.
        /// </summary>
        public virtual ProtocolDefinition GetProtocolDefinition()
        {
            var protocolDefinition = new ProtocolDefinition
            {
                ContractAssembly = _contractAssembly,
                ProducerNamespace = ProducerNamespace,
                Style = new DocLiteralStyle()
            };

            // protocolDefinition.ContractAssembly = _contractAssembly;

            foreach (var version in SupportedVersions)
                protocolDefinition.SupportedVersions.Add(version);

            foreach (var filter in EnabledFilters)
                protocolDefinition.EnabledFilters.Add(filter);

            if (!protocolDefinition.GlobalNamespacePrefixes.ContainsKey(NamespaceConstants.SoapEnv))
                protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(NamespaceConstants.SoapEnv), PrefixConstants.SoapEnv);

            if (!protocolDefinition.GlobalNamespacePrefixes.ContainsKey(NamespaceConstants.Xsd))
                protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(NamespaceConstants.Xsd), PrefixConstants.Xsd);

            if (!protocolDefinition.GlobalNamespacePrefixes.ContainsKey(NamespaceConstants.Xsi))
                protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(NamespaceConstants.Xsi), PrefixConstants.Xsi);

            if (!string.IsNullOrEmpty(protocolDefinition.ProducerNamespace) && !protocolDefinition.GlobalNamespacePrefixes.ContainsKey(protocolDefinition.ProducerNamespace))
                protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(protocolDefinition.ProducerNamespace), PrefixConstants.Target);

            return protocolDefinition;
        }
        
        /// <summary>
        /// Initializes default fault definition and applies customizations (if any).
        /// </summary>
        public virtual FaultDefinition GetFaultDefinition() =>
            new FaultDefinition { Name = XName.Get("fault", ProducerNamespace) };
        
        /// <summary>
        /// Returns `true` if given namespace defines qualified element names by default.
        /// </summary>
        public virtual bool IsQualifiedElementDefault(string namespaceName) =>
            false;
        
        /// <summary>
        /// Get schema location of specified schema namespace.
        /// </summary>
        public virtual string GetSchemaLocation(string namespaceName) =>
            NamespaceConstants.GetSchemaLocation(namespaceName);
    }
}