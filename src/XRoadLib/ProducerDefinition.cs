using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;

#if NETSTANDARD1_5
using MessageCollection = System.Collections.Generic.ICollection<System.Web.Services.Description.Message>;
using ServiceDescriptionFormatExtensionCollection = System.Collections.Generic.ICollection<System.Web.Services.Description.ServiceDescriptionFormatExtension>;
using XRoadLib.Xml.Schema;
#else
using System.Xml.Schema;
#endif

namespace XRoadLib
{
    /// <summary>
    /// Extracts contract information from given assembly and presents it as
    /// service description XML document.
    /// </summary>
    public sealed class ProducerDefinition
    {
        private readonly Assembly contractAssembly;
        private readonly IXRoadProtocol protocol;
        private readonly SchemaDefinitionProvider schemaDefinitionProvider;
        private readonly uint? version;

        private readonly Binding binding;
        private readonly PortType portType;
        private readonly Port servicePort;
        private readonly Service service;

        private readonly IDictionary<Type, List<Type>> derivedTypes = new Dictionary<Type, List<Type>>();
        private readonly IDictionary<Type, XmlQualifiedName> additionalTypeDefinitions = new Dictionary<Type, XmlQualifiedName>();
        private readonly IDictionary<XName, TypeDefinition> schemaTypeDefinitions = new Dictionary<XName, TypeDefinition>();
        private readonly IDictionary<Type, TypeDefinition> runtimeTypeDefinitions = new Dictionary<Type, TypeDefinition>();
        private readonly IDictionary<string, string> schemaLocations = new Dictionary<string, string>();

        private readonly Action<string, string> addRequiredImport;
        private readonly Func<string, IList<string>> getRequiredImports;

        private readonly Func<string, bool> addGlobalNamespace;
        private readonly Func<IList<Tuple<string, string>>> getGlobalNamespaces;

        private readonly string xRoadPrefix;
        private readonly string xRoadNamespace;
        private readonly HeaderDefinition headerDefinition;

        private readonly XmlDocument document = new XmlDocument();

        /// <summary>
        /// Initialize builder with contract details.
        /// <param name="protocol">X-Road protocol to use when generating service description.</param>
        /// <param name="schemaDefinitionProvider">Provides overrides for default presentation format.</param>
        /// <param name="version">Global version for service description (when versioning entire schema and operations using same version number).</param>
        /// </summary>
        public ProducerDefinition(IXRoadProtocol protocol, SchemaDefinitionProvider schemaDefinitionProvider, uint? version = null)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));
            this.protocol = protocol;

            if (schemaDefinitionProvider == null)
                throw new ArgumentNullException(nameof(schemaDefinitionProvider));
            this.schemaDefinitionProvider = schemaDefinitionProvider;

            this.version = version;
            contractAssembly = schemaDefinitionProvider.ProtocolDefinition.ContractAssembly;

            portType = new PortType { Name = "PortTypeName" };

            binding = new Binding
            {
                Name = "BindingName",
                Type = new XmlQualifiedName(portType.Name, protocol.ProducerNamespace)
            };

            servicePort = new Port
            {
                Name = "PortName",
                Binding = new XmlQualifiedName(binding.Name, protocol.ProducerNamespace)
            };

            service = new Service
            {
                Name = "ServiceName",
                Ports = { servicePort }
            };

            CollectTypes();

            var globalNamespaces = new Dictionary<string, string>();

            addGlobalNamespace = (namespaceName) =>
            {
                var prefix = NamespaceConstants.GetPreferredPrefix(namespaceName);
                if (string.IsNullOrEmpty(prefix))
                    return false;

                globalNamespaces[prefix] = namespaceName;

                return true;
            };

            getGlobalNamespaces = () => globalNamespaces.Select(x => Tuple.Create(x.Key, x.Value)).ToList();

            var requiredImports = new SortedSet<Tuple<string, string>>();

            addRequiredImport = (schemaNamespace, typeNamespace) =>
            {
                if (typeNamespace == NamespaceConstants.XSD || typeNamespace == schemaNamespace)
                    return;

                if (!schemaLocations.ContainsKey(typeNamespace))
                    schemaLocations.Add(typeNamespace, schemaDefinitionProvider.GetSchemaLocation(typeNamespace));

                requiredImports.Add(Tuple.Create(schemaNamespace, typeNamespace));
            };

            getRequiredImports = ns => requiredImports.Where(x => x.Item1 == ns).Select(x => x.Item2).ToList();

            xRoadPrefix = schemaDefinitionProvider.GetXRoadPrefix();
            xRoadNamespace = schemaDefinitionProvider.GetXRoadNamespace();
            headerDefinition = schemaDefinitionProvider.GetXRoadHeaderDefinition();

            addGlobalNamespace(NamespaceConstants.SOAP);
            addGlobalNamespace(NamespaceConstants.SOAP_ENV);
            addGlobalNamespace(NamespaceConstants.WSDL);
            addGlobalNamespace(NamespaceConstants.XSD);
        }

        /// <summary>
        /// Outputs service description to specified stream.
        /// </summary>
        public void SaveTo(Stream stream)
        {
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n" }))
                WriteServiceDescription(writer);
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

            var typeDefinitions = contractAssembly.GetTypes()
                                                  .Where(type => type.IsXRoadSerializable() || (type.GetTypeInfo().IsEnum && type.GetTypeInfo().GetCustomAttribute<XmlTypeAttribute>() != null))
                                                  .Where(type => !version.HasValue || type.GetTypeInfo().ExistsInVersion(version.Value))
                                                  .Select(type => schemaDefinitionProvider.GetTypeDefinition(type))
                                                  .Where(def => !def.IsAnonymous && def.Name != null && def.State == DefinitionState.Default)
                                                  .ToList();

            foreach (var typeDefinition in typeDefinitions)
            {
                if (schemaTypeDefinitions.ContainsKey(typeDefinition.Name))
                    throw new Exception($"Multiple type definitions for same name `{typeDefinition.Name}`.");

                schemaTypeDefinitions.Add(typeDefinition.Name, typeDefinition);
                runtimeTypeDefinitions.Add(typeDefinition.Type, typeDefinition);

                var baseType = typeDefinition.Type.GetTypeInfo().BaseType;
                if (baseType == null || !baseType.IsXRoadSerializable())
                    continue;

                List<Type> typeList;
                if (!derivedTypes.TryGetValue(baseType, out typeList))
                    derivedTypes.Add(baseType, typeList = new List<Type>());

                typeList.Add(typeDefinition.Type);
            }
        }

        private IEnumerable<OperationDefinition> GetOperationDefinitions(string targetNamespace)
        {
            return contractAssembly.GetServiceContracts()
                                   .SelectMany(x => x.Value
                                                     .Where(op => !version.HasValue || op.ExistsInVersion(version.Value))
                                                     .Select(op => schemaDefinitionProvider.GetOperationDefinition(x.Key, XName.Get(op.Name, targetNamespace), version)))
                                   .Where(def => def.State == DefinitionState.Default)
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
                var requestValueDefinition = schemaDefinitionProvider.GetRequestValueDefinition(operationDefinition);

                Func<XmlSchemaElement> createRequestElement = () => requestValueDefinition.ParameterInfo != null
                    ? CreateContentElement(requestValueDefinition, targetNamespace, referencedTypes)
                    : new XmlSchemaElement { Name = requestValueDefinition.RequestElementName, SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence() } };

                var requestElement = createRequestElement();

                if (protocol.Style.UseElementInMessagePart)
                    schemaElements.Add(new XmlSchemaElement
                    {
                        Name = operationDefinition.Name.LocalName,
                        SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElement } } }
                    });

                var responseValueDefinition = schemaDefinitionProvider.GetResponseValueDefinition(operationDefinition);
                if (!responseValueDefinition.ContainsNonTechnicalFault)
                    addFaultType = true;

                XmlSchemaElement responseElement;
                XmlSchemaElement resultElement = null;

                if (responseValueDefinition.XRoadFaultPresentation != XRoadFaultPresentation.Implicit && responseValueDefinition.ContainsNonTechnicalFault)
                {
                    var outputParticle = new XmlSchemaSequence();
                    responseElement = new XmlSchemaElement { Name = responseValueDefinition.ResponseElementName, SchemaType = new XmlSchemaComplexType { Particle = outputParticle } };

                    var faultSequence = CreateFaultSequence();

                    if (operationDefinition.MethodInfo.ReturnType == typeof(void))
                    {
                        faultSequence.MinOccurs = 0;
                        outputParticle.Items.Add(faultSequence);
                    }
                    else if (responseValueDefinition.XRoadFaultPresentation == XRoadFaultPresentation.Choice)
                    {
                        resultElement = CreateContentElement(responseValueDefinition, targetNamespace, referencedTypes);
                        outputParticle.Items.Add(new XmlSchemaChoice { Items = { resultElement, faultSequence } });
                    }
                    else
                    {
                        resultElement = CreateContentElement(responseValueDefinition, targetNamespace, referencedTypes);
                        resultElement.MinOccurs = 0;
                        outputParticle.Items.Add(resultElement);

                        faultSequence.MinOccurs = 0;
                        outputParticle.Items.Add(faultSequence);
                    }
                }
                else responseElement = resultElement = CreateContentElement(responseValueDefinition, targetNamespace, referencedTypes);

                if (protocol.Style.UseElementInMessagePart)
                {
                    var responseRequestElement = requestElement;
                    if (requestValueDefinition.RequestElementName != responseValueDefinition.RequestElementName)
                    {
                        responseRequestElement = createRequestElement();
                        responseRequestElement.Name = responseValueDefinition.RequestElementName;
                    }
                    schemaElements.Add(new XmlSchemaElement
                    {
                        Name = $"{operationDefinition.Name.LocalName}Response",
                        SchemaType = CreateOperationResponseSchemaType(responseValueDefinition, responseRequestElement, responseElement, faultDefinition)
                    });
                }

                if (operationDefinition.IsAbstract)
                    continue;

                var inputMessage = new Message { Name = operationDefinition.InputMessageName };

                if (protocol.Style.UseElementInMessagePart)
                    inputMessage.Parts.Add(new MessagePart { Name = "body", Element = new XmlQualifiedName(operationDefinition.Name.LocalName, operationDefinition.Name.NamespaceName) });
                else
                {
                    var requestTypeName = requestElement?.SchemaTypeName;
                    inputMessage.Parts.Add(new MessagePart { Name = requestValueDefinition.RequestElementName, Type = requestTypeName });
                }

                if (operationDefinition.InputBinaryMode == BinaryMode.Attachment)
                    inputMessage.Parts.Add(new MessagePart { Name = "file", Type = new XmlQualifiedName("base64Binary", NamespaceConstants.XSD) });

                messages.Add(inputMessage);

                var outputMessage = new Message { Name = operationDefinition.OutputMessageName };

                if (protocol.Style.UseElementInMessagePart)
                    outputMessage.Parts.Add(new MessagePart { Name = "body", Element = new XmlQualifiedName($"{operationDefinition.Name.LocalName}Response", operationDefinition.Name.NamespaceName) });
                else
                {
                    var requestTypeName = requestElement?.SchemaTypeName;
                    var responseTypeName = GetOutputMessageTypeName(resultElement, operationDefinition.MethodInfo.ReturnType, schemaTypes);
                    outputMessage.Parts.Add(new MessagePart { Name = responseValueDefinition.RequestElementName, Type = requestTypeName });
                    outputMessage.Parts.Add(new MessagePart { Name = responseValueDefinition.ResponseElementName, Type = responseTypeName });
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
                    TypeDefinition typeDefinition;
                    if (!schemaTypeDefinitions.TryGetValue(XName.Get(kvp.Key.Name, kvp.Key.Namespace), out typeDefinition))
                        continue;

                    if (typeDefinition.IsSimpleType)
                        continue;

                    XmlSchemaType schemaType;

                    if (typeDefinition.Type.GetTypeInfo().IsEnum)
                    {
                        schemaType = new XmlSchemaSimpleType();
                        AddEnumTypeContent(typeDefinition.Type, (XmlSchemaSimpleType)schemaType);
                    }
                    else
                    {
                        schemaType = new XmlSchemaComplexType { IsAbstract = typeDefinition.Type.GetTypeInfo().IsAbstract };

                        if (AddComplexTypeContent((XmlSchemaComplexType)schemaType, typeDefinition.Name.NamespaceName, typeDefinition, referencedTypes) != null)
                            throw new NotImplementedException();
                    }

                    schemaType.Name = typeDefinition.Name.LocalName;
                    schemaType.Annotation = CreateSchemaAnnotation(typeDefinition.Name.NamespaceName, typeDefinition);

                    AddSchemaType(schemaTypes, typeDefinition.Name.NamespaceName, schemaType);

                    referencedTypes[kvp.Key] = schemaType;

                    List<Type> typeList;
                    if (!derivedTypes.TryGetValue(typeDefinition.Type, out typeList))
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
                    Annotation = CreateSchemaAnnotation(faultDefinition.Name.NamespaceName, faultDefinition)
                });

            return schemaTypes
                .Select(x => x.Item1)
                .Where(x => x != NamespaceConstants.XSD && x != targetNamespace)
                .Distinct()
                .Where(ns => schemaLocations[ns] == null)
                .Select(x => BuildSchemaForNamespace(x, schemaTypes, null))
                .Concat(new [] { BuildSchemaForNamespace(targetNamespace, schemaTypes, schemaElements) })
                .ToList();
        }

        private XmlSchemaComplexType CreateOperationResponseSchemaType(ResponseValueDefinition definition, XmlSchemaElement requestElement, XmlSchemaElement responseElement, FaultDefinition faultDefinition)
        {
            if (definition.ContainsNonTechnicalFault)
                return new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElement, responseElement } } };

            if ("unbounded".Equals(responseElement.MaxOccursString) || responseElement.MaxOccurs > 1)
                responseElement = new XmlSchemaElement
                {
                    Name = definition.ResponseElementName,
                    SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { responseElement } } }
                };
            else responseElement.Name = definition.ResponseElementName;

            var complexTypeSequence = new XmlSchemaSequence();

            if (definition.DeclaringOperationDefinition.CopyRequestPartToResponse)
                complexTypeSequence.Items.Add(requestElement);

            switch (definition.XRoadFaultPresentation)
            {
                case XRoadFaultPresentation.Choice:
                    complexTypeSequence.Items.Add(new XmlSchemaChoice { Items = { responseElement, CreateFaultElement(definition, faultDefinition) } });
                    break;

                case XRoadFaultPresentation.Explicit:
                    responseElement.MinOccurs = 0;
                    var faultElement = CreateFaultElement(definition, faultDefinition);
                    faultElement.MinOccurs = 0;
                    complexTypeSequence.Items.Add(responseElement);
                    complexTypeSequence.Items.Add(faultElement);
                    break;

                case XRoadFaultPresentation.Implicit:
                    complexTypeSequence.Items.Add(responseElement);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new XmlSchemaComplexType { Particle = complexTypeSequence };
        }

        private static XmlSchemaElement CreateFaultElement(ResponseValueDefinition responseValueDefinition, FaultDefinition faultDefinition)
        {
            return new XmlSchemaElement
            {
                Name = responseValueDefinition.FaultName,
                SchemaTypeName = new XmlQualifiedName(faultDefinition.Name.LocalName, faultDefinition.Name.NamespaceName)
            };
        }

        private XmlSchemaSequence CreateFaultSequence()
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

            var schema = new XmlSchema { TargetNamespace = schemaNamespace };

            foreach (var namespaceType in namespaceTypes.OrderBy(x => x.Name.ToLower()))
                schema.Items.Add(namespaceType);

            if (schemaElements != null)
                foreach (var schemaElement in schemaElements.OrderBy(x => x.Name.ToLower()))
                    schema.Items.Add(schemaElement);

            if (schema.TargetNamespace != schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace)
                schema.Namespaces.Add("tns", schema.TargetNamespace);

            var n = 1;
            foreach (var namespaceImport in getRequiredImports(schema.TargetNamespace))
            {
                schema.Includes.Add(new XmlSchemaImport { Namespace = namespaceImport, SchemaLocation = schemaLocations[namespaceImport] });
                if (!addGlobalNamespace(namespaceImport))
                    schema.Namespaces.Add($"ns{n++}", namespaceImport);
            }

            return schema;
        }

        private XmlQualifiedName AddAdditionalTypeDefinition(Type type, string typeName, XmlSchemaElement schemaElement, IList<Tuple<string, XmlSchemaType>> schemaTypes)
        {
            XmlQualifiedName qualifiedName;
            if (additionalTypeDefinitions.TryGetValue(type, out qualifiedName))
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

            if (resultType.IsArray)
                return AddAdditionalTypeDefinition(resultType, $"ArrayOf{resultType.GetElementType().Name}", resultElement, schemaTypes);

            return null;
        }

        private void AddPortTypeOperation(OperationDefinition operationDefinition, Message inputMessage, Message outputMessage, string targetNamespace)
        {
            portType.Operations.Add(new Operation
            {
                DocumentationElement = CreateDocumentationElement(operationDefinition.Documentation),
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

#if NETSTANDARD1_5
            AddOperationContentBinding(soapPart.Extensions, x => x);
#else
            AddOperationContentBinding(soapPart.Extensions, protocol.Style.CreateSoapHeader);
#endif

            var filePart = new MimePart { Extensions = { new MimeContentBinding { Part = "file", Type = "application/binary" } } };

            messageBinding.Extensions.Add(new MimeMultipartRelatedBinding { Parts = { soapPart, filePart } });
        }

        private void AddBindingOperation(OperationDefinition operationDefinition)
        {
            var inputBinding = new InputBinding();
            AddOperationMessageBindingContent(operationDefinition.InputBinaryMode, inputBinding);

            var outputBinding = new OutputBinding();
            AddOperationMessageBindingContent(operationDefinition.OutputBinaryMode, outputBinding);

            binding.Operations.Add(new OperationBinding
            {
                Name = operationDefinition.Name.LocalName,
                Extensions =
                {
                    CreateXRoadOperationVersionBinding(operationDefinition),
                    protocol.Style.CreateSoapOperationBinding()
                },
                Input = inputBinding,
                Output = outputBinding
            });
        }

        private void AddOperationContentBinding<THeader>(ServiceDescriptionFormatExtensionCollection extensions, Func<SoapHeaderBinding, THeader> projectionFunc)
#if NETSTANDARD1_5
            where THeader : ServiceDescriptionFormatExtension
#endif
        {
            extensions.Add(protocol.Style.CreateSoapBodyBinding(protocol.ProducerNamespace));
            foreach (var headerBinding in headerDefinition.RequiredHeaders.Select(name => protocol.Style.CreateSoapHeaderBinding(name, headerDefinition.MessageName, protocol.ProducerNamespace)).Select(projectionFunc))
                extensions.Add(headerBinding);
        }

        private XmlQualifiedName AddComplexTypeContent(XmlSchemaComplexType schemaType, string schemaNamespace, TypeDefinition typeDefinition, IDictionary<XmlQualifiedName, XmlSchemaType> referencedTypes)
        {
            var contentParticle = new XmlSchemaSequence();

            var propertyDefinitions = GetDescriptionProperties(typeDefinition).ToList();

            foreach (var propertyDefinition in propertyDefinitions)
            {
                if (propertyDefinition.MergeContent && propertyDefinitions.Count > 1 && propertyDefinition.ArrayItemDefinition == null)
                    throw new Exception($"Property {propertyDefinition} of type {typeDefinition} cannot be merged, because there are more than 1 properties present.");

                var contentElement = CreateContentElement(propertyDefinition, schemaNamespace, referencedTypes);

                if (!propertyDefinition.MergeContent || propertyDefinition.ArrayItemDefinition != null)
                {
                    contentParticle.Items.Add(contentElement);
                    continue;
                }

                if (!contentElement.SchemaTypeName.IsEmpty)
                    return contentElement.SchemaTypeName;

                var particle = ((XmlSchemaComplexType)contentElement.SchemaType)?.Particle;
                if (particle != null) schemaType.Particle = particle;
                var content = ((XmlSchemaComplexType)contentElement.SchemaType)?.ContentModel;
                if (content != null) schemaType.ContentModel = content;

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

            TypeDefinition typeDefinition;
            if (!runtimeTypeDefinitions.TryGetValue(type, out typeDefinition))
                throw new Exception($"Unrecognized type `{type.FullName}`.");

            addRequiredImport(schemaNamespace, typeDefinition.Name.NamespaceName);

            return new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
        }

        private XmlSchemaAnnotation CreateSchemaAnnotation(string schemaNamespace, Definition definition)
        {
            if (!definition.Documentation.Any())
                return null;

            var markup = definition.Documentation
                                   .Select(doc => CreateTitleElement(doc.Item1, doc.Item2, ns => addRequiredImport(schemaNamespace, ns)))
                                   .Cast<XmlNode>();

#if NETSTANDARD1_5
            var appInfo = new XmlSchemaAppInfo();
            appInfo.Markup.AddRange(markup);
#else
            var appInfo = new XmlSchemaAppInfo { Markup = markup.ToArray() };
#endif

            return new XmlSchemaAnnotation { Items = { appInfo } };
        }

        private void AddBinaryAttribute(string schemaNamespace, XmlSchemaAnnotated schemaElement)
        {
            addRequiredImport(schemaNamespace, NamespaceConstants.XMIME);

#if NETSTANDARD1_5
            schemaElement.UnhandledAttributes.Add(protocol.Style.CreateExpectedContentType("application/octet-stream"));
#else
            schemaElement.UnhandledAttributes = new[] { protocol.Style.CreateExpectedContentType("application/octet-stream") };
#endif
        }

        private TypeDefinition GetContentTypeDefinition(IContentDefinition contentDefinition)
        {
            if (contentDefinition.TypeName != null)
                return schemaTypeDefinitions[contentDefinition.TypeName];

            if (runtimeTypeDefinitions.ContainsKey(contentDefinition.RuntimeType))
                return runtimeTypeDefinitions[contentDefinition.RuntimeType];

            return schemaDefinitionProvider.GetTypeDefinition(contentDefinition.RuntimeType);
        }

        private void SetSchemaElementType(XmlSchemaElement schemaElement, string schemaNamespace, IContentDefinition contentDefinition, IDictionary<XmlQualifiedName, XmlSchemaType> referencedTypes)
        {
            if (typeof(Stream).GetTypeInfo().IsAssignableFrom(contentDefinition.RuntimeType) && contentDefinition.UseXop)
                AddBinaryAttribute(schemaNamespace, schemaElement);

            var typeDefinition = GetContentTypeDefinition(contentDefinition);
            if (!typeDefinition.IsAnonymous)
            {
                addRequiredImport(schemaNamespace, typeDefinition.Name.NamespaceName);
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
                AddEnumTypeContent(contentDefinition.RuntimeType, (XmlSchemaSimpleType)schemaType);
            }
            else
            {
                schemaType = new XmlSchemaComplexType();
                schemaTypeName = AddComplexTypeContent((XmlSchemaComplexType)schemaType, schemaNamespace, typeDefinition, referencedTypes);
            }
            schemaType.Annotation = CreateSchemaAnnotation(schemaNamespace, typeDefinition);

            if (schemaTypeName == null)
                schemaElement.SchemaType = schemaType;
            else schemaElement.SchemaTypeName = schemaTypeName;
        }

        private static void AddEnumTypeContent(Type type, XmlSchemaSimpleType schemaType)
        {
            var restriction = new XmlSchemaSimpleTypeRestriction { BaseTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) };

            foreach (var name in Enum.GetNames(type))
            {
                var memberInfo = type.GetTypeInfo().GetMember(name).Single();
                var attribute = memberInfo.GetSingleAttribute<XmlEnumAttribute>();
                restriction.Facets.Add(new XmlSchemaEnumerationFacet { Value = (attribute?.Name).GetValueOrDefault(name) });
            }

            schemaType.Content = restriction;
        }

        private XmlSchemaElement CreateContentElement(ContentDefinition propertyDefinition, string schemaNamespace, IDictionary<XmlQualifiedName, XmlSchemaType> referencedTypes)
        {
            var schemaElement = new XmlSchemaElement
            {
                Name = propertyDefinition.Name?.LocalName, Annotation = CreateSchemaAnnotation(schemaNamespace, propertyDefinition)
            };

            if (propertyDefinition.ArrayItemDefinition != null && propertyDefinition.MergeContent)
            {
                schemaElement.Name = propertyDefinition.ArrayItemDefinition.Name.LocalName;

                if (propertyDefinition.ArrayItemDefinition.IsOptional)
                    schemaElement.MinOccurs = 0;

                schemaElement.IsNillable = propertyDefinition.ArrayItemDefinition.IsNullable;

                schemaElement.MaxOccursString = "unbounded";

                SetSchemaElementType(schemaElement, schemaNamespace, propertyDefinition.ArrayItemDefinition, referencedTypes);

                return schemaElement;
            }

            if (propertyDefinition.IsOptional)
                schemaElement.MinOccurs = 0;

            schemaElement.IsNillable = propertyDefinition.IsNullable;

            if (propertyDefinition.ArrayItemDefinition == null)
            {
                SetSchemaElementType(schemaElement, schemaNamespace, propertyDefinition, referencedTypes);
                return schemaElement;
            }

            if (propertyDefinition.TypeName != null)
            {
                var typeDefinition = schemaTypeDefinitions[propertyDefinition.TypeName];
                schemaElement.SchemaTypeName = new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
                if (referencedTypes.ContainsKey(schemaElement.SchemaTypeName))
                    referencedTypes.Add(schemaElement.SchemaTypeName, null);
                return schemaElement;
            }

            var itemElement = new XmlSchemaElement
            {
                Name = propertyDefinition.ArrayItemDefinition.Name.LocalName, MaxOccursString = "unbounded"
            };

            if (propertyDefinition.ArrayItemDefinition.IsOptional)
                itemElement.MinOccurs = 0;

            itemElement.IsNillable = propertyDefinition.ArrayItemDefinition.IsNullable;

            SetSchemaElementType(itemElement, schemaNamespace, propertyDefinition.ArrayItemDefinition, referencedTypes);

            protocol.Style.AddItemElementToArrayElement(schemaElement, itemElement, ns => addRequiredImport(schemaNamespace, ns));

            return schemaElement;
        }

        private void WriteServiceDescription(XmlWriter writer)
        {
            var serviceDescription = new ServiceDescription { TargetNamespace = protocol.ProducerNamespace };

            var standardHeader = new Message { Name = headerDefinition.MessageName };

            foreach (var requiredHeader in headerDefinition.RequiredHeaders)
                standardHeader.Parts.Add(new MessagePart { Name = requiredHeader.LocalName, Element = new XmlQualifiedName(requiredHeader.LocalName, requiredHeader.NamespaceName) });

            serviceDescription.Messages.Add(standardHeader);

            foreach (var schema in BuildSchemas(protocol.ProducerNamespace, serviceDescription.Messages))
                serviceDescription.Types.Schemas.Add(schema);

            serviceDescription.PortTypes.Add(portType);

            binding.Extensions.Add(protocol.Style.CreateSoapBinding());
            serviceDescription.Bindings.Add(binding);

            servicePort.Extensions.Add(new SoapAddressBinding { Location = "http://INSERT_CORRECT_SERVICE_URL" });

            serviceDescription.Services.Add(service);

            AddServiceDescriptionNamespaces(serviceDescription);

            schemaDefinitionProvider.ExportServiceDescription(serviceDescription);

            writer.WriteStartDocument();
            serviceDescription.Write(writer);
            writer.WriteEndDocument();
            writer.Flush();
        }

        private void AddServiceDescriptionNamespaces(DocumentableItem serviceDescription)
        {
            foreach (var tuple in getGlobalNamespaces())
                serviceDescription.Namespaces.Add(tuple.Item1, tuple.Item2);
            serviceDescription.Namespaces.Add("", protocol.ProducerNamespace);
        }

        private IEnumerable<PropertyDefinition> GetDescriptionProperties(TypeDefinition typeDefinition)
        {
            return typeDefinition.Type.GetPropertiesSorted(typeDefinition.ContentComparer, version, p => schemaDefinitionProvider.GetPropertyDefinition(p, typeDefinition)).Where(d => d.State == DefinitionState.Default);
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

        private
#if NETSTANDARD1_5
            XRoadOperationVersionBinding
#else
            XmlElement
#endif
                CreateXRoadOperationVersionBinding(OperationDefinition operationDefinition)
        {
            if (operationDefinition.Version == 0)
                return null;

#if NETSTANDARD1_5
            return new XRoadOperationVersionBinding(xRoadPrefix, xRoadNamespace) { Version = $"v{operationDefinition.Version}" };
#else
            var addressElement = document.CreateElement(xRoadPrefix, "version", xRoadNamespace);
            addressElement.InnerText = $"v{operationDefinition.Version}";
            return addressElement;
#endif
        }

        private XmlElement CreateTitleElement(string languageCode, string value, Action<string> addSchemaImport)
        {
            addSchemaImport(xRoadNamespace);

            var titleElement = document.CreateElement(xRoadPrefix, "title", xRoadNamespace);
            titleElement.InnerText = value;

            if (string.IsNullOrWhiteSpace(languageCode))
                return titleElement;

            var attribute = document.CreateAttribute("xml", "lang", null);
            attribute.Value = languageCode;
            titleElement.Attributes.Append(attribute);

            return titleElement;
        }

        private XmlElement CreateDocumentationElement(IList<Tuple<string, string>> titles)
        {
            if (titles == null || !titles.Any())
                return null;

            var documentationElement = document.CreateElement(PrefixConstants.WSDL, "documentation", NamespaceConstants.WSDL);

            foreach (var title in titles)
                documentationElement.AppendChild(CreateTitleElement(title.Item1, title.Item2, _ => { }));

            return documentationElement;
        }
    }
}
