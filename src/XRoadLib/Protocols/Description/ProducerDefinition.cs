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

namespace XRoadLib.Protocols.Description
{
    /// <summary>
    /// Extracts contract information from given assembly and presents it as
    /// service description XML document.
    /// </summary>
    public sealed class ProducerDefinition
    {
        private const string STANDARD_HEADER_NAME = "RequiredHeaders";

        private readonly Assembly contractAssembly;
        private readonly XRoadProtocol protocol;
        private readonly SchemaDefinitionReader schemaDefinitionReader;
        private readonly uint? version;

        private readonly Binding binding;
        private readonly PortType portType;
        private readonly Port servicePort;
        private readonly Service service;

        private readonly IDictionary<Type, List<Type>> derivedTypes = new Dictionary<Type, List<Type>>();
        private readonly IDictionary<Type, XmlQualifiedName> additionalTypeDefinitions = new Dictionary<Type, XmlQualifiedName>();
        private readonly IDictionary<XName, TypeDefinition> schemaTypeDefinitions = new Dictionary<XName, TypeDefinition>();
        private readonly IDictionary<Type, TypeDefinition> runtimeTypeDefinitions = new Dictionary<Type, TypeDefinition>();
        private readonly ISet<Tuple<string, string>> requiredImports = new SortedSet<Tuple<string, string>>();

        /// <summary>
        /// Initialize builder with contract details.
        /// <param name="protocol">X-Road protocol to use when generating service description.</param>
        /// <param name="schemaDefinitionReader">Provides overrides for default presentation format.</param>
        /// <param name="contractAssembly">Source of types and operations that define service description content.</param>
        /// <param name="version">Global version for service description (when versioning entire schema and operations using same version number).</param>
        /// </summary>
        public ProducerDefinition(XRoadProtocol protocol, SchemaDefinitionReader schemaDefinitionReader, Assembly contractAssembly, uint? version = null)
        {
            if (contractAssembly == null)
                throw new ArgumentNullException(nameof(contractAssembly));
            this.contractAssembly = contractAssembly;

            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));
            this.protocol = protocol;

            this.schemaDefinitionReader = schemaDefinitionReader;
            this.version = version;

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
                                                  .Select(type => schemaDefinitionReader.GetTypeDefinition(type))
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
                                                     .Select(op => schemaDefinitionReader.GetOperationDefinition(x.Key, XName.Get(op.Name, targetNamespace), version)))
                                   .Where(def => def.State == DefinitionState.Default)
                                   .OrderBy(def => def.Name.LocalName.ToLower())
                                   .ToList();
        }

        private IEnumerable<XmlSchema> BuildSchemas(string targetNamespace, MessageCollection messages)
        {
            var schemaTypes = new List<Tuple<string, XmlSchemaType>>();
            var schemaElements = new List<XmlSchemaElement>();
            var referencedTypes = new Dictionary<XmlQualifiedName, XmlSchemaType>();

            var faultDefinition = schemaDefinitionReader.GetFaultDefinition();

            foreach (var operationDefinition in GetOperationDefinitions(targetNamespace))
            {
                var methodParameters = operationDefinition.MethodInfo.GetParameters();
                if (methodParameters.Length > 1)
                    throw new Exception($"Invalid X-Road operation contract `{operationDefinition.Name.LocalName}`: expected 0-1 input parameters, but {methodParameters.Length} was given.");

                var parameterInfo = methodParameters.SingleOrDefault();
                var requestElement = parameterInfo != null
                    ? CreateContentElement(new RequestValueDefinition(parameterInfo, operationDefinition), targetNamespace, referencedTypes)
                    : new XmlSchemaElement { Name = "request", SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence() } };

                if (protocol.Style.UseElementInMessagePart)
                    schemaElements.Add(new XmlSchemaElement
                    {
                        Name = operationDefinition.Name.LocalName,
                        SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElement } } }
                    });

                var responseValueDefinition = schemaDefinitionReader.GetResponseValueDefinition(operationDefinition);

                XmlSchemaElement responseElement;
                XmlSchemaElement resultElement = null;

                if (responseValueDefinition.XRoadFaultPresentation != XRoadFaultPresentation.Implicit && protocol.NonTechnicalFaultInResponseElement)
                {
                    var outputParticle = new XmlSchemaSequence();
                    responseElement = new XmlSchemaElement { Name = "response", SchemaType = new XmlSchemaComplexType { Particle = outputParticle } };

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
                    schemaElements.Add(new XmlSchemaElement
                    {
                        Name = $"{operationDefinition.Name.LocalName}Response",
                        SchemaType = CreateOperationResponseSchemaType(responseValueDefinition, requestElement, responseElement, faultDefinition)
                    });

                if (operationDefinition.IsAbstract)
                    continue;

                var inputMessage = new Message { Name = operationDefinition.InputMessageName };

                if (protocol.Style.UseElementInMessagePart)
                    inputMessage.Parts.Add(new MessagePart { Name = "body", Element = new XmlQualifiedName(operationDefinition.Name.LocalName, operationDefinition.Name.NamespaceName) });
                else
                {
                    var requestTypeName = requestElement?.SchemaTypeName;
                    inputMessage.Parts.Add(new MessagePart { Name = protocol.RequestPartNameInRequest, Type = requestTypeName });
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
                    outputMessage.Parts.Add(new MessagePart { Name = protocol.RequestPartNameInResponse, Type = requestTypeName });
                    outputMessage.Parts.Add(new MessagePart { Name = protocol.ResponsePartNameInResponse, Type = responseTypeName });
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

                    schemaTypes.Add(Tuple.Create(typeDefinition.Name.NamespaceName, schemaType));

                    referencedTypes[kvp.Key] = schemaType;

                    List<Type> typeList;
                    if (!derivedTypes.TryGetValue(typeDefinition.Type, out typeList))
                        continue;

                    foreach (var qualifiedName in typeList.Select(x => runtimeTypeDefinitions[x]).Select(x => new XmlQualifiedName(x.Name.LocalName, x.Name.NamespaceName)).Where(x => !referencedTypes.ContainsKey(x)))
                        referencedTypes.Add(qualifiedName, null);
                }
            } while (initialCount != referencedTypes.Count);

            if (!protocol.NonTechnicalFaultInResponseElement && faultDefinition.State == DefinitionState.Default)
            {
                var faultType = new XmlSchemaComplexType { Name = faultDefinition.Name.LocalName, Particle = CreateFaultSequence() };
                faultType.Annotation = CreateSchemaAnnotation(faultDefinition.Name.NamespaceName, faultDefinition);
                schemaTypes.Add(Tuple.Create(faultDefinition.Name.NamespaceName, (XmlSchemaType)faultType));
            }

            var allNamespaces = schemaTypes.Select(x => x.Item1).Where(x => x != NamespaceConstants.XSD).Distinct().ToList();
            var externalNamespaces = allNamespaces.Where(x => x != targetNamespace).ToList();

            var schemas = new List<XmlSchema>();

            if (protocol.IncludeExternalSchemas)
            {
                var elements = new List<XmlSchemaElement>();
                foreach (var schema in externalNamespaces.Select(externalNamespace => BuildSchemaForNamespace(externalNamespace, schemaTypes, elements, allNamespaces)))
                {
                    schema.Namespaces.Add("ns0", schema.TargetNamespace);
                    schemas.Add(schema);
                }
            }

            schemas.Add(BuildSchemaForNamespace(targetNamespace, schemaTypes, schemaElements, allNamespaces));

            return schemas;
        }

        private XmlSchemaComplexType CreateOperationResponseSchemaType(ResponseValueDefinition definition, XmlSchemaElement requestElement, XmlSchemaElement responseElement, FaultDefinition faultDefinition)
        {
            if (protocol.NonTechnicalFaultInResponseElement)
                return new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElement, responseElement } } };

            if ("unbounded".Equals(responseElement.MaxOccursString) || responseElement.MaxOccurs > 1)
                responseElement = new XmlSchemaElement
                {
                    Name = "response",
                    SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { responseElement } } }
                };
            else responseElement.Name = "response";

            var complexTypeSequence = new XmlSchemaSequence();

            if (!definition.DeclaringOperationDefinition.ProhibitRequestPartInResponse)
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

        private XmlSchema BuildSchemaForNamespace(string schemaNamespace, IList<Tuple<string, XmlSchemaType>> schemaTypes, IList<XmlSchemaElement> schemaElements, IList<string> customNamespaces)
        {
            var namespaceTypes = schemaTypes.Where(x => x.Item1 == schemaNamespace).Select(x => x.Item2).ToList();
            var namespaceImports = requiredImports.Where(x => x.Item1 == schemaNamespace).Select(x => x.Item2).ToList();

            var schema = new XmlSchema { TargetNamespace = schemaNamespace };

            foreach (var namespaceType in namespaceTypes.OrderBy(x => x.Name.ToLower()))
                schema.Items.Add(namespaceType);

            foreach (var schemaElement in schemaElements.OrderBy(x => x.Name.ToLower()))
                schema.Items.Add(schemaElement);

            var n = 1;
            foreach (var namespaceImport in namespaceImports)
            {
                schema.Includes.Add(new XmlSchemaImport { Namespace = namespaceImport, SchemaLocation = namespaceImport });
                if (customNamespaces.Contains(namespaceImport))
                    schema.Namespaces.Add($"ns{n++}", namespaceImport);
            }

            return schema;
        }

        private XmlQualifiedName AddAdditionalTypeDefinition(Type type, string typeName, XmlSchemaElement schemaElement, IList<Tuple<string, XmlSchemaType>> schemaTypes)
        {
            XmlQualifiedName qualifiedName;
            if (additionalTypeDefinitions.TryGetValue(type, out qualifiedName))
                return qualifiedName;

            var definition = schemaDefinitionReader.GetTypeDefinition(type, typeName);
            if (definition.State != DefinitionState.Default)
                return null;

            qualifiedName = new XmlQualifiedName(definition.Name.LocalName, definition.Name.NamespaceName);
            additionalTypeDefinitions.Add(type, qualifiedName);

            var schemaType = new XmlSchemaComplexType { Name = qualifiedName.Name };
            schemaTypes.Add(Tuple.Create(qualifiedName.Namespace, (XmlSchemaType)schemaType));

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
                DocumentationElement = protocol.CreateDocumentationElement(operationDefinition.Documentation),
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
                    protocol.CreateOperationVersionElement(operationDefinition),
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
            foreach (var headerBinding in protocol.MandatoryHeaders.Select(name => protocol.Style.CreateSoapHeaderBinding(name, STANDARD_HEADER_NAME, protocol.ProducerNamespace)).Select(projectionFunc))
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

            AddRequiredImport(schemaNamespace, typeDefinition.Name.NamespaceName);

            return new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
        }

        private XmlSchemaAnnotation CreateSchemaAnnotation(string schemaNamespace, Definition definition)
        {
            if (!definition.Documentation.Any())
                return null;

            var markup = definition.Documentation
                                   .Select(doc => protocol.CreateTitleElement(doc.Item1, doc.Item2, ns => requiredImports.Add(Tuple.Create(schemaNamespace, ns))))
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
            requiredImports.Add(Tuple.Create(schemaNamespace, NamespaceConstants.XMIME));

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

            return schemaDefinitionReader.GetTypeDefinition(contentDefinition.RuntimeType);
        }

        private void AddRequiredImport(string schemaNamespace, string typeNamespace)
        {
            if (typeNamespace != NamespaceConstants.XSD && typeNamespace != schemaNamespace)
                requiredImports.Add(Tuple.Create(schemaNamespace, typeNamespace));
        }

        private void SetSchemaElementType(XmlSchemaElement schemaElement, string schemaNamespace, IContentDefinition contentDefinition, IDictionary<XmlQualifiedName, XmlSchemaType> referencedTypes)
        {
            if (typeof(Stream).GetTypeInfo().IsAssignableFrom(contentDefinition.RuntimeType) && contentDefinition.UseXop)
                AddBinaryAttribute(schemaNamespace, schemaElement);

            var typeDefinition = GetContentTypeDefinition(contentDefinition);
            if (!typeDefinition.IsAnonymous)
            {
                AddRequiredImport(schemaNamespace, typeDefinition.Name.NamespaceName);
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

            protocol.Style.AddItemElementToArrayElement(schemaElement, itemElement, ns => requiredImports.Add(Tuple.Create(schemaNamespace, ns)));

            return schemaElement;
        }

        private void WriteServiceDescription(XmlWriter writer)
        {
            var serviceDescription = new ServiceDescription { TargetNamespace = protocol.ProducerNamespace };
            AddServiceDescriptionNamespaces(serviceDescription);

            var standardHeader = new Message { Name = STANDARD_HEADER_NAME };

            foreach (var requiredHeader in protocol.MandatoryHeaders)
                standardHeader.Parts.Add(new MessagePart { Name = requiredHeader.LocalName, Element = new XmlQualifiedName(requiredHeader.LocalName, requiredHeader.NamespaceName) });

            serviceDescription.Messages.Add(standardHeader);

            foreach (var schema in BuildSchemas(protocol.ProducerNamespace, serviceDescription.Messages))
                serviceDescription.Types.Schemas.Add(schema);

            serviceDescription.PortTypes.Add(portType);

            binding.Extensions.Add(protocol.Style.CreateSoapBinding());
            serviceDescription.Bindings.Add(binding);

            servicePort.Extensions.Add(new SoapAddressBinding { Location = "" });

            serviceDescription.Services.Add(service);

            protocol.ExportServiceDescription(serviceDescription);

            writer.WriteStartDocument();
            serviceDescription.Write(writer);
            writer.WriteEndDocument();
            writer.Flush();
        }

        private void AddServiceDescriptionNamespaces(DocumentableItem serviceDescription)
        {
            serviceDescription.Namespaces.Add(PrefixConstants.MIME, NamespaceConstants.MIME);
            serviceDescription.Namespaces.Add(PrefixConstants.SOAP, NamespaceConstants.SOAP);
            serviceDescription.Namespaces.Add(PrefixConstants.SOAP_ENV, NamespaceConstants.SOAP_ENV);
            serviceDescription.Namespaces.Add(PrefixConstants.WSDL, NamespaceConstants.WSDL);
            serviceDescription.Namespaces.Add(PrefixConstants.XMIME, NamespaceConstants.XMIME);
            serviceDescription.Namespaces.Add(PrefixConstants.XSD, NamespaceConstants.XSD);
            serviceDescription.Namespaces.Add("", protocol.ProducerNamespace);
        }

        private IEnumerable<PropertyDefinition> GetDescriptionProperties(TypeDefinition typeDefinition)
        {
            return typeDefinition.Type.GetPropertiesSorted(typeDefinition.ContentComparer, version, p => schemaDefinitionReader.GetPropertyDefinition(p, typeDefinition)).Where(d => d.State == DefinitionState.Default);
        }

        private void AddSystemType<T>(string typeName)
        {
            var typeDefinition = schemaDefinitionReader.GetSimpleTypeDefinition<T>(typeName);

            if (typeDefinition.Type != null && !runtimeTypeDefinitions.ContainsKey(typeDefinition.Type))
                runtimeTypeDefinitions.Add(typeDefinition.Type, typeDefinition);

            if (typeDefinition.Name != null && !schemaTypeDefinitions.ContainsKey(typeDefinition.Name))
                schemaTypeDefinitions.Add(typeDefinition.Name, typeDefinition);
        }
    }
}
