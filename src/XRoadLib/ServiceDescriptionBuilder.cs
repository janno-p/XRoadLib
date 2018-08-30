using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Wsdl;
using MessageCollection = System.Collections.Generic.ICollection<XRoadLib.Wsdl.Message>;
using ServiceDescriptionFormatExtensionCollection = System.Collections.Generic.ICollection<XRoadLib.Wsdl.ServiceDescriptionFormatExtension>;

namespace XRoadLib
{
    /// <summary>
    /// Extracts contract information from given assembly and presents it as
    /// service description XML document.
    /// </summary>
    public sealed class ServiceDescriptionBuilder
    {
        private readonly SchemaDefinitionProvider schemaDefinitionProvider;
        private readonly uint? version;
        private readonly Func<OperationDefinition, bool> operationFilter;

        private readonly Binding binding;
        private readonly PortType portType;
        private readonly Port servicePort;
        private readonly Service service;

        private readonly IDictionary<Type, List<Type>> derivedTypes = new Dictionary<Type, List<Type>>();
        private readonly IDictionary<Type, XmlQualifiedName> additionalTypeDefinitions = new Dictionary<Type, XmlQualifiedName>();
        private readonly IDictionary<XName, TypeDefinition> schemaTypeDefinitions = new Dictionary<XName, TypeDefinition>();
        private readonly IDictionary<Type, TypeDefinition> runtimeTypeDefinitions = new Dictionary<Type, TypeDefinition>();
        private readonly IDictionary<string, string> schemaLocations = new Dictionary<string, string>();

        private readonly Action<string, string, ISchemaExporter> addRequiredImport;
        private readonly Func<string, IList<string>> getRequiredImports;
        private readonly Func<IList<string>> getImportedSchemas;

        private readonly Func<string, bool> addGlobalNamespace;
        private readonly Func<IList<Tuple<string, string>>> getGlobalNamespaces;

        private readonly string xRoadPrefix;
        private readonly string xRoadNamespace;
        private readonly HeaderDefinition headerDefinition;

        private readonly XmlDocument document = new XmlDocument();

        /// <summary>
        /// Initialize builder with contract details.
        /// <param name="schemaDefinitionProvider">Provides overrides for default presentation format.</param>
        /// <param name="operationFilter">Allows to filter operations which are presented in service description.</param>
        /// <param name="version">Global version for service description (when versioning entire schema and operations using same version number).</param>
        /// </summary>
        public ServiceDescriptionBuilder(SchemaDefinitionProvider schemaDefinitionProvider, Func<OperationDefinition, bool> operationFilter = null, uint? version = null)
        {
            this.schemaDefinitionProvider = schemaDefinitionProvider ?? throw new ArgumentNullException(nameof(schemaDefinitionProvider));
            this.operationFilter = operationFilter;
            this.version = version;

            var protocolDefinition = schemaDefinitionProvider.ProtocolDefinition;

            portType = new PortType { Name = protocolDefinition.PortTypeName };

            var producerNamespace = protocolDefinition.ProducerNamespace;

            binding = new Binding
            {
                Name = protocolDefinition.BindingName,
                Type = new XmlQualifiedName(portType.Name, producerNamespace)
            };

            servicePort = new Port
            {
                Name = protocolDefinition.PortName,
                Binding = new XmlQualifiedName(binding.Name, producerNamespace)
            };

            service = new Service
            {
                Name = protocolDefinition.ServiceName,
                Ports = { servicePort }
            };

            CollectTypes();

            var globalNamespaces = new Dictionary<string, string>();

            addGlobalNamespace = namespaceName =>
            {
                var prefix = NamespaceConstants.GetPreferredPrefix(namespaceName);
                if (string.IsNullOrEmpty(prefix))
                    return false;

                globalNamespaces[prefix] = namespaceName;

                return true;
            };

            getGlobalNamespaces = () => globalNamespaces.Select(x => Tuple.Create(x.Key, x.Value)).ToList();

            var requiredImports = new SortedSet<Tuple<string, string>>();

            addRequiredImport = (schemaNamespace, typeNamespace, schemaExporter) =>
            {
                if (typeNamespace == NamespaceConstants.XSD || typeNamespace == schemaNamespace)
                    return;

                if (!schemaLocations.ContainsKey(typeNamespace))
                    schemaLocations.Add(typeNamespace, schemaDefinitionProvider.GetSchemaLocation(typeNamespace, schemaExporter));

                requiredImports.Add(Tuple.Create(schemaNamespace, typeNamespace));
            };

            getRequiredImports = ns => requiredImports.Where(x => x.Item1 == ns).Select(x => x.Item2).ToList();

            getImportedSchemas = () => requiredImports.Select(x => x.Item2).Where(x => x != null && schemaLocations[x] != null).Distinct().ToList();

            xRoadPrefix = schemaDefinitionProvider.GetXRoadPrefix();
            xRoadNamespace = schemaDefinitionProvider.GetXRoadNamespace();
            headerDefinition = schemaDefinitionProvider.GetXRoadHeaderDefinition();

            addGlobalNamespace(NamespaceConstants.SOAP);
            addGlobalNamespace(NamespaceConstants.WSDL);
            addGlobalNamespace(NamespaceConstants.XSD);
        }

        private void CollectTypes()
        {
            AddSystemType<DateTime>("dateTime");
            AddSystemType<DateTime>("date");

            AddSystemType<bool>("boolean");

            AddSystemType<float>("float");
            AddSystemType<double>("double");
            AddSystemType<decimal>("decimal");

            AddSystemType<long>("long");
            AddSystemType<int>("int");
            AddSystemType<short>("short");
            AddSystemType<BigInteger>("integer");

            AddSystemType<string>("string");
            AddSystemType<string>("anyURI");

            AddSystemType<Stream>("base64Binary");
            AddSystemType<Stream>("hexBinary");
            AddSystemType<Stream>("base64");

            var typeDefinitions =
                schemaDefinitionProvider
                    .ProtocolDefinition
                    .ContractAssembly
                    .GetTypes()
                    .Where(type => type.IsXRoadSerializable() || (type.GetTypeInfo().IsEnum && type.GetTypeInfo().GetCustomAttribute<XmlTypeAttribute>() != null))
                    .Where(type => !version.HasValue || type.GetTypeInfo().ExistsInVersion(version.Value))
                    .Select(type => schemaDefinitionProvider.GetTypeDefinition(type))
                    .Where(def => !def.IsAnonymous && def.Name != null && def.State == DefinitionState.Default)
                    .ToList();

            foreach (var typeDefinition in typeDefinitions)
            {
                if (schemaTypeDefinitions.ContainsKey(typeDefinition.Name))
                    throw new SchemaDefinitionException($"Multiple type definitions for same name `{typeDefinition.Name}`.");

                schemaTypeDefinitions.Add(typeDefinition.Name, typeDefinition);
                runtimeTypeDefinitions.Add(typeDefinition.Type, typeDefinition);

                var baseType = typeDefinition.Type.GetTypeInfo().BaseType;
                if (baseType == null || !baseType.IsXRoadSerializable())
                    continue;

                if (!derivedTypes.TryGetValue(baseType, out var typeList))
                    derivedTypes.Add(baseType, typeList = new List<Type>());

                typeList.Add(typeDefinition.Type);
            }
        }

        private IEnumerable<OperationDefinition> GetOperationDefinitions(string targetNamespace)
        {
            return schemaDefinitionProvider
                   .ProtocolDefinition
                   .ContractAssembly
                   .GetServiceContracts()
                   .SelectMany(x => x.Value
                                     .Where(op => !version.HasValue || op.ExistsInVersion(version.Value))
                                     .Select(op => schemaDefinitionProvider.GetOperationDefinition(x.Key, XName.Get(op.Name, targetNamespace), version)))
                   .Where(operationFilter ?? (def => def.State == DefinitionState.Default))
                   .OrderBy(def => def.Name.LocalName.ToLower())
                   .ToList();
        }

        private IEnumerable<XmlSchema> BuildSchemas(string targetNamespace, MessageCollection messages)
        {
            var schemaTypes = new List<Tuple<string, XmlSchemaType>>();
            var schemaElements = new List<XmlSchemaElement>();
            var referencedTypes = new Dictionary<XmlQualifiedName, XmlSchemaType>();

            var faultDefinition = schemaDefinitionProvider.GetFaultDefinition();
            var addFaultType = false;

            foreach (var operationDefinition in GetOperationDefinitions(targetNamespace))
            {
                var requestDefinition = schemaDefinitionProvider.GetRequestDefinition(operationDefinition);

                XmlSchemaElement CreateRequestElement() => requestDefinition.ParameterInfo != null
                    ? CreateContentElement(requestDefinition.Content, targetNamespace, referencedTypes)
                    : new XmlSchemaElement { Name = requestDefinition.RequestElementName.LocalName, SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence() } };

                var requestElement = CreateRequestElement();
                AddCustomAttributes(requestDefinition.Content, requestElement, ns => addRequiredImport(targetNamespace, ns, operationDefinition.ExtensionSchemaExporter));

                var requestWrapperElementName = requestDefinition.WrapperElementName;

                if (schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                {
                    if (requestDefinition.Content.MergeContent)
                    {
                        requestElement.Name = requestWrapperElementName.LocalName;
                        schemaElements.Add(requestElement);
                    }
                    else
                    {
                        schemaElements.Add(new XmlSchemaElement
                        {
                            Name = requestWrapperElementName.LocalName,
                            SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElement } } }
                        });
                    }
                }

                var responseDefinition = schemaDefinitionProvider.GetResponseDefinition(operationDefinition);
                if (!responseDefinition.ContainsNonTechnicalFault)
                    addFaultType = true;

                XmlSchemaElement responseElement;
                XmlSchemaElement resultElement = null;

                var responseWrapperElementName = responseDefinition.WrapperElementName;

                if (responseDefinition.XRoadFaultPresentation != XRoadFaultPresentation.Implicit && responseDefinition.ContainsNonTechnicalFault)
                {
                    var outputParticle = new XmlSchemaSequence();
                    responseElement = new XmlSchemaElement { Name = responseDefinition.ResponseElementName.LocalName, SchemaType = new XmlSchemaComplexType { Particle = outputParticle } };

                    var faultSequence = CreateFaultSequence();

                    if (operationDefinition.MethodInfo.ReturnType == typeof(void))
                    {
                        faultSequence.MinOccurs = 0;
                        outputParticle.Items.Add(faultSequence);
                    }
                    else if (responseDefinition.XRoadFaultPresentation == XRoadFaultPresentation.Choice)
                    {
                        resultElement = CreateContentElement(responseDefinition.Content, targetNamespace, referencedTypes);
                        outputParticle.Items.Add(new XmlSchemaChoice { Items = { resultElement, faultSequence } });
                    }
                    else
                    {
                        resultElement = CreateContentElement(responseDefinition.Content, targetNamespace, referencedTypes);
                        resultElement.MinOccurs = 0;
                        outputParticle.Items.Add(resultElement);

                        faultSequence.MinOccurs = 0;
                        outputParticle.Items.Add(faultSequence);
                    }
                }
                else responseElement = resultElement = CreateContentElement(responseDefinition.Content, targetNamespace, referencedTypes);

                AddCustomAttributes(responseDefinition.Content, responseElement, ns => addRequiredImport(targetNamespace, ns, operationDefinition.ExtensionSchemaExporter));

                if (schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                {
                    var responseRequestElement = requestElement;
                    if (requestDefinition.RequestElementName != responseDefinition.RequestElementName)
                    {
                        responseRequestElement = CreateRequestElement();
                        responseRequestElement.Name = responseDefinition.RequestElementName.LocalName;
                    }
                    schemaElements.Add(new XmlSchemaElement
                    {
                        Name = responseWrapperElementName.LocalName,
                        SchemaType = CreateOperationResponseSchemaType(responseDefinition, responseRequestElement, responseElement, faultDefinition)
                    });
                }

                if (operationDefinition.IsAbstract)
                    continue;

                var inputMessage = new Message { Name = operationDefinition.InputMessageName };

                if (schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                    inputMessage.Parts.Add(new MessagePart { Name = "body", Element = new XmlQualifiedName(requestWrapperElementName.LocalName, requestWrapperElementName.NamespaceName) });
                else
                {
                    var requestTypeName = requestElement?.SchemaTypeName;
                    inputMessage.Parts.Add(new MessagePart { Name = requestDefinition.RequestElementName.LocalName, Type = requestTypeName });
                }

                if (operationDefinition.InputBinaryMode == BinaryMode.Attachment)
                    inputMessage.Parts.Add(new MessagePart { Name = "file", Type = new XmlQualifiedName("base64Binary", NamespaceConstants.XSD) });

                messages.Add(inputMessage);

                var outputMessage = new Message { Name = operationDefinition.OutputMessageName };

                if (schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                    outputMessage.Parts.Add(new MessagePart { Name = "body", Element = new XmlQualifiedName(responseWrapperElementName.LocalName, responseWrapperElementName.NamespaceName) });
                else
                {
                    var requestTypeName = requestElement?.SchemaTypeName;
                    var responseTypeName = GetOutputMessageTypeName(resultElement, operationDefinition.MethodInfo.ReturnType, schemaTypes);
                    outputMessage.Parts.Add(new MessagePart { Name = responseDefinition.RequestElementName.LocalName, Type = requestTypeName });
                    outputMessage.Parts.Add(new MessagePart { Name = responseDefinition.ResponseElementName.LocalName, Type = responseTypeName });
                }

                if (operationDefinition.OutputBinaryMode == BinaryMode.Attachment)
                    outputMessage.Parts.Add(new MessagePart { Name = "file", Type = new XmlQualifiedName("base64Binary", NamespaceConstants.XSD) });

                messages.Add(outputMessage);

                AddPortTypeOperation(operationDefinition, inputMessage, outputMessage, targetNamespace);

                AddBindingOperation(operationDefinition);
            }

            int initialCount;
            do
            {
                initialCount = referencedTypes.Count;

                foreach (var kvp in referencedTypes.ToList().Where(x => x.Value == null))
                {
                    if (!schemaTypeDefinitions.TryGetValue(XName.Get(kvp.Key.Name, kvp.Key.Namespace), out var typeDefinition))
                        continue;

                    if (typeDefinition.IsSimpleType)
                        continue;

                    XmlSchemaType schemaType;

                    if (typeDefinition.Type.GetTypeInfo().IsEnum)
                    {
                        schemaType = new XmlSchemaSimpleType();
                        AddEnumTypeContent(typeDefinition.Type.Namespace, typeDefinition.Type, (XmlSchemaSimpleType)schemaType);
                    }
                    else
                    {
                        schemaType = new XmlSchemaComplexType { IsAbstract = typeDefinition.Type.GetTypeInfo().IsAbstract };

                        if (AddComplexTypeContent((XmlSchemaComplexType)schemaType, typeDefinition.Name.NamespaceName, typeDefinition, referencedTypes) != null)
                            throw new NotImplementedException();
                    }

                    schemaType.Name = typeDefinition.Name.LocalName;
                    schemaType.Annotation = CreateSchemaAnnotation(typeDefinition.Name.NamespaceName, typeDefinition.Documentation, schemaType.Name);

                    AddSchemaType(schemaTypes, typeDefinition.Name.NamespaceName, schemaType);

                    referencedTypes[kvp.Key] = schemaType;

                    if (!derivedTypes.TryGetValue(typeDefinition.Type, out var typeList))
                        continue;

                    foreach (var qualifiedName in typeList.Select(x => runtimeTypeDefinitions[x]).Select(x => new XmlQualifiedName(x.Name.LocalName, x.Name.NamespaceName)).Where(x => !referencedTypes.ContainsKey(x)))
                        referencedTypes.Add(qualifiedName, null);
                }
            } while (initialCount != referencedTypes.Count);

            if (addFaultType && faultDefinition.State == DefinitionState.Default)
                AddSchemaType(schemaTypes, faultDefinition.Name.NamespaceName, new XmlSchemaComplexType
                {
                    Name = faultDefinition.Name.LocalName,
                    Particle = CreateFaultSequence(),
                    Annotation = CreateSchemaAnnotation(faultDefinition.Name.NamespaceName, faultDefinition.Documentation, faultDefinition.Name.LocalName)
                });

            var schemas = schemaTypes
                .Select(x => x.Item1)
                .Where(x => x != NamespaceConstants.XSD && x != targetNamespace)
                .Distinct()
                .Where(ns => schemaLocations[ns] == null)
                .Select(x => BuildSchemaForNamespace(x, schemaTypes, null))
                .Concat(new [] { BuildSchemaForNamespace(targetNamespace, schemaTypes, schemaElements) })
                .ToList();

            var importSchema = GetNamespaceImportSchema();
            if (importSchema != null)
                schemas.Insert(0, importSchema);

            return schemas;
        }

        private XmlSchema GetNamespaceImportSchema()
        {
            var importedSchemas = getImportedSchemas();
            if (!importedSchemas.Any())
                return null;

            var schema = CreateXmlSchema();
            foreach (var importedSchema in importedSchemas)
                schema.Includes.Add(new XmlSchemaImport { Namespace = importedSchema, SchemaLocation = schemaLocations[importedSchema] });

            return schema;
        }

        private static XmlSchema CreateXmlSchema()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(PrefixConstants.XSD, NamespaceConstants.XSD);
            return new XmlSchema { Namespaces = namespaces };
        }

        private static XmlSchemaComplexType CreateOperationResponseSchemaType(ResponseDefinition responseDefinition, XmlSchemaElement requestElement, XmlSchemaElement responseElement, FaultDefinition faultDefinition)
        {
            if (responseDefinition.ContainsNonTechnicalFault)
            {
                var particle = new XmlSchemaSequence();

                if (responseDefinition.DeclaringOperationDefinition.CopyRequestPartToResponse)
                    particle.Items.Add(requestElement);

                particle.Items.Add(responseElement);

                return new XmlSchemaComplexType { Particle = particle };
            }

            if ("unbounded".Equals(responseElement.MaxOccursString) || responseElement.MaxOccurs > 1)
                responseElement = new XmlSchemaElement
                {
                    Name = responseDefinition.ResponseElementName.LocalName,
                    SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { responseElement } } }
                };
            else responseElement.Name = responseDefinition.ResponseElementName.LocalName;

            var complexTypeSequence = new XmlSchemaSequence();

            if (responseDefinition.DeclaringOperationDefinition.CopyRequestPartToResponse)
                complexTypeSequence.Items.Add(requestElement);

            switch (responseDefinition.XRoadFaultPresentation)
            {
                case XRoadFaultPresentation.Choice:
                    complexTypeSequence.Items.Add(new XmlSchemaChoice { Items = { responseElement, CreateFaultElement(responseDefinition, faultDefinition) } });
                    break;

                case XRoadFaultPresentation.Explicit:
                    responseElement.MinOccurs = 0;
                    var faultElement = CreateFaultElement(responseDefinition, faultDefinition);
                    faultElement.MinOccurs = 0;
                    complexTypeSequence.Items.Add(responseElement);
                    complexTypeSequence.Items.Add(faultElement);
                    break;

                case XRoadFaultPresentation.Implicit:
                    complexTypeSequence.Items.Add(responseElement);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new XmlSchemaComplexType { Particle = complexTypeSequence };
        }

        private static XmlSchemaElement CreateFaultElement(ResponseDefinition responseDefinition, FaultDefinition faultDefinition)
        {
            return new XmlSchemaElement
            {
                Name = responseDefinition.FaultName,
                SchemaTypeName = new XmlQualifiedName(faultDefinition.Name.LocalName, faultDefinition.Name.NamespaceName)
            };
        }

        private static XmlSchemaSequence CreateFaultSequence()
        {
            return new XmlSchemaSequence
            {
                Items =
                {
                    new XmlSchemaElement { Name = "faultCode", SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) },
                    new XmlSchemaElement { Name = "faultString", SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) }
                }
            };
        }

        private XmlSchema BuildSchemaForNamespace(string schemaNamespace, IList<Tuple<string, XmlSchemaType>> schemaTypes, IList<XmlSchemaElement> schemaElements)
        {
            var namespaceTypes = schemaTypes.Where(x => x.Item1 == schemaNamespace).Select(x => x.Item2).ToList();

            var schema = CreateXmlSchema();
            schema.TargetNamespace = schemaNamespace;

            if (schemaDefinitionProvider.IsQualifiedElementDefault(schemaNamespace))
                schema.ElementFormDefault = XmlSchemaForm.Qualified;

            foreach (var namespaceType in namespaceTypes.OrderBy(x => x.Name.ToLower()))
                schema.Items.Add(namespaceType);

            if (schemaElements != null)
                foreach (var schemaElement in schemaElements.OrderBy(x => x.Name.ToLower()))
                    schema.Items.Add(schemaElement);

            if (schema.TargetNamespace != schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace)
                schema.Namespaces.Add("tns", schema.TargetNamespace);

            var n = 1;
            foreach (var namespaceImport in getRequiredImports(schema.TargetNamespace).Where(ns => !addGlobalNamespace(ns)))
                schema.Namespaces.Add($"ns{n++}", namespaceImport);

            return schema;
        }

        private XmlQualifiedName AddAdditionalTypeDefinition(Type type, string typeName, XmlSchemaElement schemaElement, IList<Tuple<string, XmlSchemaType>> schemaTypes)
        {
            if (additionalTypeDefinitions.TryGetValue(type, out var qualifiedName))
                return qualifiedName;

            var definition = schemaDefinitionProvider.GetTypeDefinition(type, typeName);
            if (definition.State != DefinitionState.Default)
                return null;

            qualifiedName = new XmlQualifiedName(definition.Name.LocalName, definition.Name.NamespaceName);
            additionalTypeDefinitions.Add(type, qualifiedName);

            var schemaType = new XmlSchemaComplexType { Name = qualifiedName.Name };
            AddSchemaType(schemaTypes, qualifiedName.Namespace, schemaType);

            if (schemaElement == null)
                return qualifiedName;

            var elementType = (XmlSchemaComplexType)schemaElement.SchemaType;
            if (elementType.Particle != null)
                schemaType.Particle = elementType.Particle;
            if (elementType.ContentModel != null)
                schemaType.ContentModel = elementType.ContentModel;

            return qualifiedName;
        }

        private XmlQualifiedName GetOutputMessageTypeName(XmlSchemaElement resultElement, Type resultType, IList<Tuple<string, XmlSchemaType>> schemaTypes)
        {
            if (resultType == typeof(void))
                return AddAdditionalTypeDefinition(resultType, "Void", resultElement, schemaTypes);

            if (!resultElement.SchemaTypeName.IsEmpty)
                return resultElement.SchemaTypeName;

            return resultType.IsArray
                ? AddAdditionalTypeDefinition(resultType, $"ArrayOf{resultType.GetElementType()?.Name ?? "Unknown"}", resultElement, schemaTypes)
                : null;
        }

        private void AddPortTypeOperation(OperationDefinition operationDefinition, Message inputMessage, Message outputMessage, string targetNamespace)
        {
            portType.Operations.Add(new Operation
            {
                DocumentationElement = CreateDocumentationElement(operationDefinition.Documentation, operationDefinition.Name.LocalName),
                Name = operationDefinition.Name.LocalName,
                Messages =
                {
                    new OperationInput { Message = new XmlQualifiedName(inputMessage.Name, targetNamespace) },
                    new OperationOutput { Message = new XmlQualifiedName(outputMessage.Name, targetNamespace) }
                }
            });
        }

        private void AddOperationMessageBindingContent(BinaryMode binaryMode, MessageBinding messageBinding)
        {
            if (binaryMode != BinaryMode.Attachment)
            {
                AddOperationContentBinding(messageBinding.Extensions, x => x);
                return;
            }

            var soapPart = new MimePart();

            addGlobalNamespace(NamespaceConstants.MIME);

            AddOperationContentBinding(soapPart.Extensions, x => x);

            var filePart = new MimePart { Extensions = { new MimeContentBinding { Part = "file", Type = "application/binary" } } };

            messageBinding.Extensions.Add(new MimeMultipartRelatedBinding { Parts = { soapPart, filePart } });
        }

        private void AddBindingOperation(OperationDefinition operationDefinition)
        {
            var inputBinding = new InputBinding();
            AddOperationMessageBindingContent(operationDefinition.InputBinaryMode, inputBinding);

            var outputBinding = new OutputBinding();
            AddOperationMessageBindingContent(operationDefinition.OutputBinaryMode, outputBinding);

            var operationBinding = new OperationBinding
            {
                Name = operationDefinition.Name.LocalName,
                Input = inputBinding,
                Output = outputBinding
            };

            var operationVersionBinding = CreateXRoadOperationVersionBinding(operationDefinition);
            if (operationVersionBinding != null)
                operationBinding.Extensions.Add(operationVersionBinding);

            operationBinding.Extensions.Add(schemaDefinitionProvider.ProtocolDefinition.Style.CreateSoapOperationBinding(operationDefinition.SoapAction));

            binding.Operations.Add(operationBinding);
        }

        private void AddOperationContentBinding<THeader>(ServiceDescriptionFormatExtensionCollection extensions, Func<SoapHeaderBinding, THeader> projectionFunc)
            where THeader : ServiceDescriptionFormatExtension
        {
            extensions.Add(schemaDefinitionProvider.ProtocolDefinition.Style.CreateSoapBodyBinding(schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace));
            foreach (var headerBinding in headerDefinition.RequiredHeaders.Select(name => schemaDefinitionProvider.ProtocolDefinition.Style.CreateSoapHeaderBinding(name, headerDefinition.MessageName, schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace)).Select(projectionFunc))
                extensions.Add(headerBinding);
        }

        private XmlQualifiedName AddComplexTypeContent(XmlSchemaComplexType schemaType, string schemaNamespace, TypeDefinition typeDefinition, IDictionary<XmlQualifiedName, XmlSchemaType> referencedTypes)
        {
            var contentParticle = new XmlSchemaSequence();

            var propertyDefinitions = GetDescriptionProperties(typeDefinition).ToList();

            foreach (var propertyDefinition in propertyDefinitions)
            {
                var propertyContent = propertyDefinition.Content;

                if (propertyContent.MergeContent && propertyDefinitions.Count > 1 && propertyContent is SingularContentDefinition)
                    throw new SchemaDefinitionException($"Property {propertyDefinition} of type {typeDefinition} cannot be merged, because there are more than 1 properties present.");

                var contentElement = CreateContentElement(propertyContent, schemaNamespace, referencedTypes);

                if (!propertyContent.MergeContent || propertyContent is ArrayContentDefiniton)
                {
                    contentParticle.Items.Add(contentElement);
                    continue;
                }

                if (!contentElement.SchemaTypeName.IsEmpty)
                    return contentElement.SchemaTypeName;

                var particle = ((XmlSchemaComplexType)contentElement.SchemaType)?.Particle;
                if (particle != null)
                    schemaType.Particle = particle;

                var content = ((XmlSchemaComplexType)contentElement.SchemaType)?.ContentModel;
                if (content != null)
                    schemaType.ContentModel = content;

                return null;
            }

            if (typeDefinition.Type.GetTypeInfo().BaseType == typeof(XRoadSerializable))
                schemaType.Particle = contentParticle;
            else
            {
                var extension = new XmlSchemaComplexContentExtension
                {
                    BaseTypeName = GetSchemaTypeName(typeDefinition.Type.GetTypeInfo().BaseType, schemaNamespace),
                    Particle = contentParticle
                };

                if (!referencedTypes.ContainsKey(extension.BaseTypeName))
                    referencedTypes.Add(extension.BaseTypeName, null);

                schemaType.ContentModel = new XmlSchemaComplexContent { Content = extension };
            }

            return null;
        }

        private XmlQualifiedName GetSchemaTypeName(Type type, string schemaNamespace)
        {
            var name = type.GetSystemTypeName();
            if (name != null)
                return new XmlQualifiedName(name.LocalName, name.NamespaceName);

            if (!runtimeTypeDefinitions.TryGetValue(type, out var typeDefinition))
                throw new SchemaDefinitionException($"Unrecognized type `{type.FullName}`.");

            addRequiredImport(schemaNamespace, typeDefinition.Name.NamespaceName, null);

            return new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
        }

        private IList<XRoadTitleAttribute> GetTitlesOrDefault(DocumentationDefinition documentationDefinition, string defaultTitle)
        {
            if (documentationDefinition.Titles.Any())
                return documentationDefinition.Titles;

            var titles = new List<XRoadTitleAttribute>();

            if (schemaDefinitionProvider.ProtocolDefinition.GenerateFakeXRoadDocumentation)
                titles.Add(new XRoadTitleAttribute(defaultTitle));

            return titles;
        }

        private XmlSchemaAnnotation CreateSchemaAnnotation(string schemaNamespace, DocumentationDefinition documentationDefinition, string defaultTitle)
        {
            if (documentationDefinition.IsEmpty && (!schemaDefinitionProvider.ProtocolDefinition.GenerateFakeXRoadDocumentation || string.IsNullOrWhiteSpace(defaultTitle)))
                return null;

            var markup =
                GetTitlesOrDefault(documentationDefinition, defaultTitle)
                    .Select(doc => CreateXrdDocumentationElement("title", doc.LanguageCode, doc.Value, ns => addRequiredImport(schemaNamespace, ns, null)))
                    .Concat(documentationDefinition
                            .Notes
                            .Select(doc => CreateXrdDocumentationElement("notes", doc.LanguageCode, doc.Value, ns => addRequiredImport(schemaNamespace, ns, null))))
                    .Concat(documentationDefinition
                            .TechNotes
                            .Select(doc => CreateXrdDocumentationElement(schemaDefinitionProvider.ProtocolDefinition.TechNotesElementName, doc.LanguageCode, doc.Value, ns => addRequiredImport(schemaNamespace, ns, null))))
                    .Cast<XmlNode>();

            var appInfo = new XmlSchemaAppInfo { Markup = markup.ToArray() };

            return new XmlSchemaAnnotation { Items = { appInfo } };
        }

        private void AddBinaryAttribute(string schemaNamespace, XmlSchemaAnnotated schemaElement)
        {
            addRequiredImport(schemaNamespace, NamespaceConstants.XMIME, null);

            schemaElement.UnhandledAttributes = new[] { schemaDefinitionProvider.ProtocolDefinition.Style.CreateExpectedContentType("application/octet-stream") };
        }

        private TypeDefinition GetContentTypeDefinition(ContentDefinition contentDefinition)
        {
            if (contentDefinition.TypeName != null)
                return schemaTypeDefinitions[contentDefinition.TypeName];

            if (runtimeTypeDefinitions.ContainsKey(contentDefinition.RuntimeType))
                return runtimeTypeDefinitions[contentDefinition.RuntimeType];

            return schemaDefinitionProvider.GetTypeDefinition(contentDefinition.RuntimeType);
        }

        private void SetSchemaElementType(XmlSchemaElement schemaElement, string schemaNamespace, ContentDefinition contentDefinition, IDictionary<XmlQualifiedName, XmlSchemaType> referencedTypes)
        {
            if (typeof(Stream).GetTypeInfo().IsAssignableFrom(contentDefinition.RuntimeType) && contentDefinition.UseXop)
                AddBinaryAttribute(schemaNamespace, schemaElement);

            var typeDefinition = GetContentTypeDefinition(contentDefinition);
            if (!typeDefinition.IsAnonymous)
            {
                if (contentDefinition.RuntimeType == typeof(object))
                    return;

                addRequiredImport(schemaNamespace, typeDefinition.Name.NamespaceName, null);
                schemaElement.SchemaTypeName = new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);

                if (!referencedTypes.ContainsKey(schemaElement.SchemaTypeName))
                    referencedTypes.Add(schemaElement.SchemaTypeName, null);

                return;
            }

            XmlSchemaType schemaType;
            XmlQualifiedName schemaTypeName = null;

            if (contentDefinition.RuntimeType.GetTypeInfo().IsEnum)
            {
                schemaType = new XmlSchemaSimpleType();
                AddEnumTypeContent(schemaNamespace, contentDefinition.RuntimeType, (XmlSchemaSimpleType)schemaType);
            }
            else
            {
                schemaType = new XmlSchemaComplexType();
                schemaTypeName = AddComplexTypeContent((XmlSchemaComplexType)schemaType, schemaNamespace, typeDefinition, referencedTypes);
            }

            schemaType.Annotation = CreateSchemaAnnotation(schemaNamespace, typeDefinition.Documentation, schemaTypeName?.Name ?? contentDefinition.RuntimeType.Name);

            if (schemaTypeName == null)
                schemaElement.SchemaType = schemaType;
            else schemaElement.SchemaTypeName = schemaTypeName;
        }

        private void AddEnumTypeContent(string schemaNamespace, Type type, XmlSchemaSimpleType schemaType)
        {
            var restriction = new XmlSchemaSimpleTypeRestriction { BaseTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) };

            foreach (var name in Enum.GetNames(type))
            {
                var memberInfo = type.GetTypeInfo().GetMember(name).Single();
                var attribute = memberInfo.GetSingleAttribute<XmlEnumAttribute>();
                var value = (attribute?.Name).GetValueOrDefault(name);
                restriction.Facets.Add(new XmlSchemaEnumerationFacet
                {
                    Annotation = CreateSchemaAnnotation(schemaNamespace, new DocumentationDefinition(memberInfo), value),
                    Value = value
                });
            }

            schemaType.Content = restriction;
        }

        private XmlSchemaElement CreateContentElement(ContentDefinition contentDefinition, string schemaNamespace, IDictionary<XmlQualifiedName, XmlSchemaType> referencedTypes)
        {
            var schemaElement = new XmlSchemaElement
            {
                Name = contentDefinition.Name?.LocalName
            };

            var elementForm =
                schemaDefinitionProvider.IsQualifiedElementDefault(schemaNamespace)
                    ? (contentDefinition.Name?.NamespaceName == "" ? (XmlSchemaForm?)XmlSchemaForm.Unqualified : null)
                    : (contentDefinition.Name?.NamespaceName == schemaNamespace ? (XmlSchemaForm?)XmlSchemaForm.Qualified : null);

            if (elementForm.HasValue)
                schemaElement.Form = elementForm.Value;

            var arrayContentDefinition = contentDefinition as ArrayContentDefiniton;

            if (arrayContentDefinition != null && contentDefinition.MergeContent)
            {
                schemaElement.Name = arrayContentDefinition.Item.Content.Name.LocalName;
                schemaElement.Annotation = CreateSchemaAnnotation(schemaNamespace, contentDefinition.Documentation, schemaElement.Name);

                if (arrayContentDefinition.Item.MinOccurs != 1u)
                    schemaElement.MinOccurs = arrayContentDefinition.Item.MinOccurs;
                
                if (!arrayContentDefinition.Item.MaxOccurs.HasValue)
                    schemaElement.MaxOccursString = "unbounded";
                else if (arrayContentDefinition.Item.MaxOccurs.Value != 1u)
                    schemaElement.MaxOccurs = arrayContentDefinition.Item.MaxOccurs.Value;

                schemaElement.IsNillable = arrayContentDefinition.Item.Content.IsNullable;

                SetSchemaElementType(schemaElement, schemaNamespace, arrayContentDefinition.Item.Content, referencedTypes);

                return schemaElement;
            }

            schemaElement.Annotation = CreateSchemaAnnotation(schemaNamespace, contentDefinition.Documentation, schemaElement.Name);

            if (contentDefinition.IsOptional)
                schemaElement.MinOccurs = 0;

            schemaElement.IsNillable = contentDefinition.IsNullable;

            if (arrayContentDefinition == null)
            {
                SetSchemaElementType(schemaElement, schemaNamespace, contentDefinition, referencedTypes);
                return schemaElement;
            }

            if (contentDefinition.TypeName != null)
            {
                var typeDefinition = schemaTypeDefinitions[contentDefinition.TypeName];
                schemaElement.SchemaTypeName = new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
                if (referencedTypes.ContainsKey(schemaElement.SchemaTypeName))
                    referencedTypes.Add(schemaElement.SchemaTypeName, null);
                return schemaElement;
            }

            var itemElement = new XmlSchemaElement
            {
                Name = arrayContentDefinition.Item.Content.Name.LocalName
            };

            if (arrayContentDefinition.Item.MinOccurs != 1)
                itemElement.MinOccurs = arrayContentDefinition.Item.MinOccurs;
                
            if (!arrayContentDefinition.Item.MaxOccurs.HasValue)
                itemElement.MaxOccursString = "unbounded";
            else if (arrayContentDefinition.Item.MaxOccurs.Value != 1u)
                itemElement.MaxOccurs = arrayContentDefinition.Item.MaxOccurs.Value;

            itemElement.IsNillable = arrayContentDefinition.Item.Content.IsNullable;

            SetSchemaElementType(itemElement, schemaNamespace, arrayContentDefinition.Item.Content, referencedTypes);

            schemaDefinitionProvider.ProtocolDefinition.Style.AddItemElementToArrayElement(schemaElement, itemElement, ns => addRequiredImport(schemaNamespace, ns, null));

            return schemaElement;
        }

        public ServiceDescription GetServiceDescription()
        {
            var protocolDefinition = schemaDefinitionProvider.ProtocolDefinition;
            var serviceDescription = new ServiceDescription { TargetNamespace = protocolDefinition.ProducerNamespace };

            var standardHeader = new Message { Name = headerDefinition.MessageName };

            foreach (var requiredHeader in headerDefinition.RequiredHeaders)
            {
                standardHeader.Parts.Add(new MessagePart { Name = requiredHeader.LocalName, Element = new XmlQualifiedName(requiredHeader.LocalName, requiredHeader.NamespaceName) });
                addRequiredImport(serviceDescription.TargetNamespace, requiredHeader.NamespaceName, null);
            }

            serviceDescription.Messages.Add(standardHeader);

            foreach (var schema in BuildSchemas(protocolDefinition.ProducerNamespace, serviceDescription.Messages))
                serviceDescription.Types.Schemas.Add(schema);

            serviceDescription.PortTypes.Add(portType);

            binding.Extensions.Add(protocolDefinition.Style.CreateSoapBinding());
            serviceDescription.Bindings.Add(binding);

            servicePort.Extensions.Add(new SoapAddressBinding { Location = protocolDefinition.SoapAddressLocation });

            serviceDescription.Services.Add(service);

            AddServiceDescriptionNamespaces(serviceDescription);

            schemaDefinitionProvider.ExportServiceDescription(serviceDescription);

            return serviceDescription;
        }

        private void AddServiceDescriptionNamespaces(DocumentableItem serviceDescription)
        {
            foreach (var tuple in getGlobalNamespaces())
                serviceDescription.Namespaces.Add(tuple.Item1, tuple.Item2);
            serviceDescription.Namespaces.Add(PrefixConstants.TARGET, schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace);
        }

        private IEnumerable<PropertyDefinition> GetDescriptionProperties(TypeDefinition typeDefinition)
        {
            return typeDefinition.Type.GetPropertiesSorted(typeDefinition.ContentComparer, version, p => schemaDefinitionProvider.GetPropertyDefinition(p, typeDefinition)).Where(d => d.Content.State == DefinitionState.Default);
        }

        private void AddSystemType<T>(string typeName)
        {
            var typeDefinition = schemaDefinitionProvider.GetSimpleTypeDefinition<T>(typeName);

            if (typeDefinition.Type != null && !runtimeTypeDefinitions.ContainsKey(typeDefinition.Type))
                runtimeTypeDefinitions.Add(typeDefinition.Type, typeDefinition);

            if (typeDefinition.Name != null && !schemaTypeDefinitions.ContainsKey(typeDefinition.Name))
                schemaTypeDefinitions.Add(typeDefinition.Name, typeDefinition);
        }

        private void AddSchemaType(ICollection<Tuple<string, XmlSchemaType>> schemaTypes, string namespaceName, XmlSchemaType schemaType)
        {
            if (!schemaLocations.ContainsKey(namespaceName))
                schemaLocations.Add(namespaceName, schemaDefinitionProvider.GetSchemaLocation(namespaceName));

            schemaTypes.Add(Tuple.Create(namespaceName, schemaType));
        }

        private XRoadOperationVersionBinding CreateXRoadOperationVersionBinding(OperationDefinition operationDefinition)
        {
            return operationDefinition.Version == 0 ?
                null :
                new XRoadOperationVersionBinding(xRoadPrefix, xRoadNamespace) { Version = $"v{operationDefinition.Version}" };
        }

        private XmlElement CreateXrdDocumentationElement(string name, string languageCode, string value, Action<string> addSchemaImport)
        {
            addSchemaImport(xRoadNamespace);

            var titleElement = document.CreateElement(xRoadPrefix, name, xRoadNamespace);
            titleElement.InnerText = value;

            if (string.IsNullOrWhiteSpace(languageCode))
                return titleElement;

            var attribute = document.CreateAttribute("xml", "lang", null);
            attribute.Value = languageCode;
            titleElement.Attributes.Append(attribute);

            return titleElement;
        }

        private XmlElement CreateDocumentationElement(DocumentationDefinition documentationDefinition, string defaultTitle)
        {
            if (documentationDefinition.IsEmpty && (!schemaDefinitionProvider.ProtocolDefinition.GenerateFakeXRoadDocumentation || string.IsNullOrWhiteSpace(defaultTitle)))
                return null;

            var documentationElement = document.CreateElement(PrefixConstants.WSDL, "documentation", NamespaceConstants.WSDL);

            foreach (var title in GetTitlesOrDefault(documentationDefinition, defaultTitle))
                documentationElement.AppendChild(CreateXrdDocumentationElement("title", title.LanguageCode, title.Value, _ => { }));

            foreach (var title in documentationDefinition.Notes)
                documentationElement.AppendChild(CreateXrdDocumentationElement("notes", title.LanguageCode, title.Value, _ => { }));

            foreach (var title in documentationDefinition.TechNotes)
                documentationElement.AppendChild(CreateXrdDocumentationElement(schemaDefinitionProvider.ProtocolDefinition.TechNotesElementName, title.LanguageCode, title.Value, _ => { }));

            return documentationElement;
        }

        private XmlAttribute CreateAttribute(XName attributeName, string value, Action<string> addSchemaImport)
        {
            addSchemaImport(attributeName.NamespaceName);

            var attribute = document.CreateAttribute(attributeName.LocalName, attributeName.NamespaceName);
            attribute.Value = value;
            return attribute;
        }

        private void AddCustomAttributes(Definition definition, XmlSchemaAnnotated target, Action<string> addSchemaImport)
        {
            if (definition.CustomAttributes == null)
                return;

            var attributes = new List<XmlAttribute>(target.UnhandledAttributes);

            foreach (var attribute in definition.CustomAttributes ?? Enumerable.Empty<Tuple<XName, string>>())
            {
                var attr = CreateAttribute(attribute.Item1, attribute.Item2, addSchemaImport);
                attributes.Add(attr);
            }

            target.UnhandledAttributes = attributes.ToArray();
        }
    }
}