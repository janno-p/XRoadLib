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
        private readonly SchemaDefinitionProvider _schemaDefinitionProvider;
        private readonly uint? _version;
        private readonly Func<OperationDefinition, bool> _operationFilter;

        private readonly Binding _binding;
        private readonly PortType _portType;
        private readonly Port _servicePort;
        private readonly Service _service;

        private readonly IDictionary<Type, List<Type>> _derivedTypes = new Dictionary<Type, List<Type>>();
        private readonly IDictionary<Type, XmlQualifiedName> _additionalTypeDefinitions = new Dictionary<Type, XmlQualifiedName>();
        private readonly IDictionary<XName, TypeDefinition> _schemaTypeDefinitions = new Dictionary<XName, TypeDefinition>();
        private readonly IDictionary<Type, TypeDefinition> _runtimeTypeDefinitions = new Dictionary<Type, TypeDefinition>();
        private readonly IDictionary<string, string> _schemaLocations = new Dictionary<string, string>();

        private readonly Action<string, string, ISchemaExporter> _addRequiredImport;
        private readonly Func<string, IList<string>> _getRequiredImports;
        private readonly Func<IList<string>> _getImportedSchemas;

        private readonly Func<string, bool> _addGlobalNamespace;
        private readonly Func<IList<Tuple<string, string>>> _getGlobalNamespaces;

        private readonly string _xRoadPrefix;
        private readonly string _xRoadNamespace;
        private readonly IHeaderDefinition _headerDefinition;

        private readonly XmlDocument _document = new XmlDocument();

        /// <summary>
        /// Initialize builder with contract details.
        /// <param name="schemaDefinitionProvider">Provides overrides for default presentation format.</param>
        /// <param name="operationFilter">Allows to filter operations which are presented in service description.</param>
        /// <param name="version">Global version for service description (when versioning entire schema and operations using same version number).</param>
        /// </summary>
        public ServiceDescriptionBuilder(SchemaDefinitionProvider schemaDefinitionProvider, Func<OperationDefinition, bool> operationFilter = null, uint? version = null)
        {
            _schemaDefinitionProvider = schemaDefinitionProvider ?? throw new ArgumentNullException(nameof(schemaDefinitionProvider));
            _operationFilter = operationFilter;
            _version = version;

            var protocolDefinition = schemaDefinitionProvider.ProtocolDefinition;

            _portType = new PortType { Name = protocolDefinition.PortTypeName };

            var producerNamespace = protocolDefinition.ProducerNamespace;

            _binding = new Binding
            {
                Name = protocolDefinition.BindingName,
                Type = new XmlQualifiedName(_portType.Name, producerNamespace)
            };

            _servicePort = new Port
            {
                Name = protocolDefinition.PortName,
                Binding = new XmlQualifiedName(_binding.Name, producerNamespace)
            };

            _service = new Service
            {
                Name = protocolDefinition.ServiceName,
                Ports = { _servicePort }
            };

            CollectTypes();

            var globalNamespaces = new Dictionary<string, string>();

            _addGlobalNamespace = namespaceName =>
            {
                var prefix = NamespaceConstants.GetPreferredPrefix(namespaceName);
                if (string.IsNullOrEmpty(prefix))
                    return false;

                globalNamespaces[prefix] = namespaceName;

                return true;
            };

            _getGlobalNamespaces = () => globalNamespaces.Select(x => Tuple.Create(x.Key, x.Value)).ToList();

            var requiredImports = new SortedSet<Tuple<string, string>>();

            _addRequiredImport = (schemaNamespace, typeNamespace, schemaExporter) =>
            {
                if (typeNamespace == NamespaceConstants.Xsd || typeNamespace == schemaNamespace)
                    return;

                if (!_schemaLocations.ContainsKey(typeNamespace))
                    _schemaLocations.Add(typeNamespace, schemaDefinitionProvider.GetSchemaLocation(typeNamespace, schemaExporter));

                requiredImports.Add(Tuple.Create(schemaNamespace, typeNamespace));
            };

            _getRequiredImports = ns => requiredImports.Where(x => x.Item1 == ns).Select(x => x.Item2).ToList();

            _getImportedSchemas = () => requiredImports.Select(x => x.Item2).Where(x => x != null && _schemaLocations[x] != null).Distinct().ToList();

            _xRoadPrefix = schemaDefinitionProvider.GetXRoadPrefix();
            _xRoadNamespace = schemaDefinitionProvider.GetXRoadNamespace();
            _headerDefinition = schemaDefinitionProvider.GetXRoadHeaderDefinition();

            _addGlobalNamespace(NamespaceConstants.Soap);
            _addGlobalNamespace(NamespaceConstants.Wsdl);
            _addGlobalNamespace(NamespaceConstants.Xsd);
        }

        private void CollectTypes()
        {
            AddSystemType<DateTime>("dateTime");
            AddSystemType<DateTime>("date");

            AddSystemType<TimeSpan>("duration");

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
                _schemaDefinitionProvider
                    .ProtocolDefinition
                    .ContractAssembly
                    .GetTypes()
                    .Where(type => type.IsXRoadSerializable())
                    .Where(type => !_version.HasValue || type.ExistsInVersion(_version.Value))
                    .Select(type => _schemaDefinitionProvider.GetTypeDefinition(type))
                    .Where(def => !def.IsAnonymous && def.Name != null && def.State == DefinitionState.Default)
                    .ToList();

            foreach (var typeDefinition in typeDefinitions)
            {
                if (_schemaTypeDefinitions.ContainsKey(typeDefinition.Name))
                    throw new SchemaDefinitionException($"Multiple type definitions for same name `{typeDefinition.Name}`.");

                _schemaTypeDefinitions.Add(typeDefinition.Name, typeDefinition);
                _runtimeTypeDefinitions.Add(typeDefinition.Type, typeDefinition);

                var baseType = typeDefinition.Type.BaseType;
                if (baseType == null || !baseType.IsXRoadSerializable())
                    continue;

                if (!_derivedTypes.TryGetValue(baseType, out var typeList))
                    _derivedTypes.Add(baseType, typeList = new List<Type>());

                typeList.Add(typeDefinition.Type);
            }
        }

        private IEnumerable<OperationDefinition> GetOperationDefinitions(string targetNamespace)
        {
            return _schemaDefinitionProvider
                   .ProtocolDefinition
                   .ContractAssembly
                   .GetServiceContracts()
                   .SelectMany(x => x.Value
                                     .Where(op => !_version.HasValue || op.ExistsInVersion(_version.Value))
                                     .Select(op => _schemaDefinitionProvider.GetOperationDefinition(x.Key, XName.Get(op.Name, targetNamespace), _version)))
                   .Where(_operationFilter ?? (def => def.State == DefinitionState.Default))
                   .OrderBy(def => def.Name.LocalName.ToLower())
                   .ToList();
        }

        private IEnumerable<XmlSchema> BuildSchemas(string targetNamespace, MessageCollection messages)
        {
            var builtTypes = new HashSet<XName>();

            var schemaTypes = new List<Tuple<string, XmlSchemaType>>();
            var schemaElements = new List<Tuple<string, XmlSchemaElement>>();
            var schemaReferences = new Dictionary<string, SchemaReferences>();

            var faultDefinition = _schemaDefinitionProvider.GetFaultDefinition();
            var addFaultType = false;

            SchemaReferences GetSchemaReferences(string ns)
            {
                if (schemaReferences.TryGetValue(ns, out var sr))
                    return sr;

                sr = new SchemaReferences(ns);
                schemaReferences.Add(ns, sr);

                return sr;
            }

            void ResolveType(XmlQualifiedName qualifiedName, XmlSchemaType type)
            {
                foreach (var sr in schemaReferences)
                    if (sr.Value.Types.ContainsKey(qualifiedName))
                        sr.Value.Types[qualifiedName] = type;
            }

            var targetSchemaReferences = GetSchemaReferences(targetNamespace);

            foreach (var operationDefinition in GetOperationDefinitions(targetNamespace))
            {
                var requestDefinition = _schemaDefinitionProvider.GetRequestDefinition(operationDefinition);

                Tuple<XmlSchemaElement, XmlSchemaElement> CreateRequestElement()
                {
                    if (requestDefinition.ParameterInfo != null)
                        return CreateContentElement(requestDefinition.Content, targetSchemaReferences);

                    var elements = CreateXmlSchemaElement(requestDefinition.Content.Name, targetSchemaReferences);
                    elements.Item1.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence() };

                    return elements;
                }

                var requestElements = CreateRequestElement();
                AddCustomAttributes(requestDefinition.Content, requestElements.Item1, ns => _addRequiredImport(targetNamespace, ns, operationDefinition.ExtensionSchemaExporter));

                var requestWrapperElementName = requestDefinition.WrapperElementName;

                if (_schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                {
                    if (requestDefinition.Content.MergeContent)
                    {
                        requestElements = SetXmlSchemaElementName(requestElements, requestWrapperElementName, targetSchemaReferences);
                        var ns = requestWrapperElementName.NamespaceName != "" ? requestWrapperElementName.NamespaceName : targetNamespace;
                        schemaElements.Add(Tuple.Create(ns, requestElements.Item1));
                    }
                    else
                    {
                        var e = CreateXmlSchemaElement(requestWrapperElementName, targetSchemaReferences);
                        e.Item1.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElements.Item2 } } };
                        var ns = requestWrapperElementName.NamespaceName != "" ? requestWrapperElementName.NamespaceName : targetNamespace;
                        schemaElements.Add(Tuple.Create(ns, e.Item1));
                    }
                }

                var responseDefinition = _schemaDefinitionProvider.GetResponseDefinition(operationDefinition);
                if (!responseDefinition.ContainsNonTechnicalFault)
                    addFaultType = true;

                var responseElements = CreateContentElement(responseDefinition.Content, targetSchemaReferences);
                Tuple<XmlSchemaElement, XmlSchemaElement> resultElements = null;

                if (responseDefinition.XRoadFaultPresentation != XRoadFaultPresentation.Implicit && responseDefinition.ContainsNonTechnicalFault)
                {
                    var outputParticle = new XmlSchemaSequence();

                    var contentElements = responseElements;

                    responseElements = CreateXmlSchemaElement(responseDefinition.Content.Name, targetSchemaReferences);
                    responseElements.Item1.SchemaType = new XmlSchemaComplexType { Particle = outputParticle };

                    var faultSequence = CreateFaultSequence(targetSchemaReferences);

                    if (operationDefinition.MethodInfo.ReturnType == typeof(void))
                    {
                        faultSequence.MinOccurs = 0;
                        outputParticle.Items.Add(faultSequence);
                    }
                    else if (responseDefinition.XRoadFaultPresentation == XRoadFaultPresentation.Choice)
                    {
                        resultElements = SetXmlSchemaElementName(contentElements, responseDefinition.ResultElementName, targetSchemaReferences);

                        outputParticle.Items.Add(new XmlSchemaChoice { Items = { resultElements.Item2, faultSequence } });
                    }
                    else
                    {
                        resultElements = SetXmlSchemaElementName(contentElements, responseDefinition.ResultElementName, targetSchemaReferences);

                        resultElements.Item2.MinOccurs = 0;
                        outputParticle.Items.Add(resultElements.Item2);

                        faultSequence.MinOccurs = 0;
                        outputParticle.Items.Add(faultSequence);
                    }
                }

                AddCustomAttributes(responseDefinition.Content, responseElements.Item1, ns => _addRequiredImport(targetNamespace, ns, operationDefinition.ExtensionSchemaExporter));

                if (_schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                {
                    var responseRequestElements = requestElements;

                    if (requestDefinition.Content.Name != responseDefinition.RequestContentName)
                    {
                        responseRequestElements = CreateRequestElement();
                        responseRequestElements = SetXmlSchemaElementName(responseRequestElements, responseDefinition.RequestContentName, targetSchemaReferences);
                    }

                    var e = CreateXmlSchemaElement(responseDefinition.WrapperElementName, targetSchemaReferences);
                    e.Item1.SchemaType = CreateOperationResponseSchemaType(responseDefinition, responseRequestElements.Item2, responseElements, faultDefinition, targetSchemaReferences);

                    var ns = responseDefinition.WrapperElementName.NamespaceName != "" ? responseDefinition.WrapperElementName.NamespaceName : targetNamespace;
                    schemaElements.Add(Tuple.Create(ns, e.Item1));
                }

                if (operationDefinition.IsAbstract)
                    continue;

                var inputMessage = new Message { Name = operationDefinition.InputMessageName };

                if (_schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                    inputMessage.Parts.Add(new MessagePart { Name = "body", Element = new XmlQualifiedName(requestWrapperElementName.LocalName, requestWrapperElementName.NamespaceName) });
                else
                {
                    var requestTypeName = requestElements.Item1.SchemaTypeName;
                    inputMessage.Parts.Add(new MessagePart { Name = requestDefinition.Content.Name.LocalName, Type = requestTypeName });
                }

                if (operationDefinition.InputBinaryMode == BinaryMode.Attachment)
                    inputMessage.Parts.Add(new MessagePart { Name = "file", Type = new XmlQualifiedName("base64Binary", NamespaceConstants.Xsd) });

                messages.Add(inputMessage);

                var outputMessage = new Message { Name = operationDefinition.OutputMessageName };

                if (_schemaDefinitionProvider.ProtocolDefinition.Style.UseElementInMessagePart)
                    outputMessage.Parts.Add(new MessagePart { Name = "body", Element = new XmlQualifiedName(responseDefinition.WrapperElementName.LocalName, responseDefinition.WrapperElementName.NamespaceName) });
                else
                {
                    var requestTypeName = requestElements.Item1.SchemaTypeName;
                    var responseTypeName = GetOutputMessageTypeName((resultElements ?? responseElements).Item1, operationDefinition.MethodInfo.ReturnType, schemaTypes);
                    outputMessage.Parts.Add(new MessagePart { Name = responseDefinition.RequestContentName.LocalName, Type = requestTypeName });
                    outputMessage.Parts.Add(new MessagePart { Name = responseDefinition.Content.Name.LocalName, Type = responseTypeName });
                }

                if (operationDefinition.OutputBinaryMode == BinaryMode.Attachment)
                    outputMessage.Parts.Add(new MessagePart { Name = "file", Type = new XmlQualifiedName("base64Binary", NamespaceConstants.Xsd) });

                messages.Add(outputMessage);

                AddPortTypeOperation(operationDefinition, inputMessage, outputMessage, targetNamespace);

                AddBindingOperation(operationDefinition);
            }

            var newTypesResolved = true;
            while (newTypesResolved)
            {
                newTypesResolved = false;

                foreach (var kvp in schemaReferences.SelectMany(x => x.Value.Types).Where(x => x.Value == null).ToList())
                {
                    var schemaTypeName = XName.Get(kvp.Key.Name, kvp.Key.Namespace);

                    if (!_schemaTypeDefinitions.TryGetValue(schemaTypeName, out var typeDefinition))
                        continue;

                    if (typeDefinition.IsSimpleType || !builtTypes.Add(schemaTypeName))
                        continue;

                    XmlSchemaType schemaType;

                    var namespaceSchemaReferences = GetSchemaReferences(typeDefinition.Name.NamespaceName);

                    if (typeDefinition.Type.IsEnum)
                    {
                        schemaType = new XmlSchemaSimpleType();
                        AddEnumTypeContent(typeDefinition.Type.Namespace, typeDefinition.Type, (XmlSchemaSimpleType)schemaType);
                    }
                    else
                    {
                        schemaType = new XmlSchemaComplexType { IsAbstract = typeDefinition.Type.IsAbstract };

                        if (AddComplexTypeContent((XmlSchemaComplexType)schemaType, typeDefinition, namespaceSchemaReferences) != null)
                            throw new NotImplementedException();
                    }

                    schemaType.Name = typeDefinition.Name.LocalName;
                    schemaType.Annotation = CreateSchemaAnnotation(typeDefinition.Name.NamespaceName, typeDefinition.Documentation, schemaType.Name);

                    AddSchemaType(schemaTypes, typeDefinition.Name.NamespaceName, schemaType);

                    ResolveType(kvp.Key, schemaType);
                    newTypesResolved = true;

                    if (!_derivedTypes.TryGetValue(typeDefinition.Type, out var typeList))
                        continue;

                    foreach (var qualifiedName in typeList.Select(x => _runtimeTypeDefinitions[x]).Select(x => new XmlQualifiedName(x.Name.LocalName, x.Name.NamespaceName)).Where(x => !namespaceSchemaReferences.Types.ContainsKey(x)))
                        namespaceSchemaReferences.Types.Add(qualifiedName, null);
                }
            }

            foreach (var kvp in schemaReferences.SelectMany(x => x.Value.Elements))
                if (!schemaElements.Any(x => x.Item1 == kvp.Key.Namespace && x.Item2 == kvp.Value))
                    schemaElements.Add(Tuple.Create(kvp.Key.Namespace, kvp.Value));

            if (addFaultType && faultDefinition.State == DefinitionState.Default)
                AddSchemaType(schemaTypes, faultDefinition.Name.NamespaceName, new XmlSchemaComplexType
                {
                    Name = faultDefinition.Name.LocalName,
                    Particle = CreateFaultSequence(targetSchemaReferences),
                    Annotation = CreateSchemaAnnotation(faultDefinition.Name.NamespaceName, faultDefinition.Documentation, faultDefinition.Name.LocalName)
                });

            var schemas = schemaTypes
                .Select(x => x.Item1)
                .Where(x => x != NamespaceConstants.Xsd && x != targetNamespace)
                .Distinct()
                .Where(ns => _schemaLocations[ns] == null)
                .Select(x => BuildSchemaForNamespace(x, schemaTypes, schemaElements))
                .Concat(new [] { BuildSchemaForNamespace(targetNamespace, schemaTypes, schemaElements) })
                .ToList();

            var importSchema = GetNamespaceImportSchema();
            if (importSchema != null)
                schemas.Insert(0, importSchema);

            return schemas;
        }

        private XmlSchema GetNamespaceImportSchema()
        {
            var importedSchemas = _getImportedSchemas();
            if (!importedSchemas.Any())
                return null;

            var schema = CreateXmlSchema();
            foreach (var importedSchema in importedSchemas)
                schema.Includes.Add(new XmlSchemaImport { Namespace = importedSchema, SchemaLocation = _schemaLocations[importedSchema] });

            return schema;
        }

        private static XmlSchema CreateXmlSchema()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(PrefixConstants.Xsd, NamespaceConstants.Xsd);
            return new XmlSchema { Namespaces = namespaces };
        }

        private XmlSchemaComplexType CreateOperationResponseSchemaType(ResponseDefinition responseDefinition, XmlSchemaElement requestElement, Tuple<XmlSchemaElement, XmlSchemaElement> responseElements, FaultDefinition faultDefinition, SchemaReferences schemaReferences)
        {
            if (responseDefinition.ContainsNonTechnicalFault)
            {
                var particle = new XmlSchemaSequence();

                if (responseDefinition.DeclaringOperationDefinition.CopyRequestPartToResponse)
                    particle.Items.Add(requestElement);

                particle.Items.Add(responseElements.Item2);

                return new XmlSchemaComplexType { Particle = particle };
            }

            if ("unbounded".Equals(responseElements.Item2.MaxOccursString) || responseElements.Item2.MaxOccurs > 1)
            {
                var resultElements = responseElements;
                responseElements = CreateXmlSchemaElement(responseDefinition.Content.Name, schemaReferences);
                responseElements.Item1.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { resultElements.Item2 } } };
            }
            else
            {
                responseElements = SetXmlSchemaElementName(responseElements, responseDefinition.Content.Name, schemaReferences);
            }

            var complexTypeSequence = new XmlSchemaSequence();

            if (responseDefinition.DeclaringOperationDefinition.CopyRequestPartToResponse)
                complexTypeSequence.Items.Add(requestElement);

            switch (responseDefinition.XRoadFaultPresentation)
            {
                case XRoadFaultPresentation.Choice:
                    var choiceFaultElements = CreateFaultElement(responseDefinition, faultDefinition, schemaReferences);
                    complexTypeSequence.Items.Add(new XmlSchemaChoice { Items = { responseElements.Item2, choiceFaultElements.Item2 } });
                    break;

                case XRoadFaultPresentation.Explicit:
                    responseElements.Item2.MinOccurs = 0;
                    var explicitFaultElements = CreateFaultElement(responseDefinition, faultDefinition, schemaReferences);
                    explicitFaultElements.Item2.MinOccurs = 0;
                    complexTypeSequence.Items.Add(responseElements.Item2);
                    complexTypeSequence.Items.Add(explicitFaultElements.Item2);
                    break;

                case XRoadFaultPresentation.Implicit:
                    complexTypeSequence.Items.Add(responseElements.Item2);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new XmlSchemaComplexType { Particle = complexTypeSequence };
        }

        private Tuple<XmlSchemaElement, XmlSchemaElement> CreateFaultElement(ResponseDefinition responseDefinition, FaultDefinition faultDefinition, SchemaReferences schemaReferences)
        {
            var elements = CreateXmlSchemaElement(responseDefinition.FaultName, schemaReferences);

            elements.Item1.SchemaTypeName = new XmlQualifiedName(faultDefinition.Name.LocalName, faultDefinition.Name.NamespaceName);

            return elements;
        }

        private XmlSchemaSequence CreateFaultSequence(SchemaReferences schemaReferences)
        {
            var faultCodeElement = CreateXmlSchemaElement(XName.Get("faultCode"), schemaReferences).Item1;
            faultCodeElement.SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.Xsd);

            var faultStringElement = CreateXmlSchemaElement(XName.Get("faultString"), schemaReferences).Item1;
            faultStringElement.SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.Xsd);

            return new XmlSchemaSequence { Items = { faultCodeElement, faultStringElement } };
        }

        private XmlSchema BuildSchemaForNamespace(string schemaNamespace, IList<Tuple<string, XmlSchemaType>> schemaTypes, IList<Tuple<string, XmlSchemaElement>> schemaElements)
        {
            var namespaceTypes = schemaTypes.Where(x => x.Item1 == schemaNamespace).Select(x => x.Item2).ToList();
            var namespaceElements = schemaElements.Where(x => x.Item1 == schemaNamespace).Select(x => x.Item2).ToList();

            var schema = CreateXmlSchema();
            schema.TargetNamespace = schemaNamespace;

            if (_schemaDefinitionProvider.IsQualifiedElementDefault(schemaNamespace))
                schema.ElementFormDefault = XmlSchemaForm.Qualified;

            foreach (var namespaceType in namespaceTypes.OrderBy(x => x.Name.ToLower()))
                schema.Items.Add(namespaceType);

            foreach (var schemaElement in namespaceElements.OrderBy(x => x.Name.ToLower()))
            {
                schemaElement.Form = XmlSchemaForm.None;
                schema.Items.Add(schemaElement);
            }

            if (schema.TargetNamespace != _schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace)
                schema.Namespaces.Add("tns", schema.TargetNamespace);

            var n = 1;
            foreach (var namespaceImport in _getRequiredImports(schema.TargetNamespace).Where(ns => !_addGlobalNamespace(ns)))
                schema.Namespaces.Add($"ns{n++}", namespaceImport);

            return schema;
        }

        private XmlQualifiedName AddAdditionalTypeDefinition(Type type, string typeName, XmlSchemaElement schemaElement, IList<Tuple<string, XmlSchemaType>> schemaTypes)
        {
            if (_additionalTypeDefinitions.TryGetValue(type, out var qualifiedName))
                return qualifiedName;

            var definition = _schemaDefinitionProvider.GetTypeDefinition(type, typeName);
            if (definition.State != DefinitionState.Default)
                return null;

            qualifiedName = new XmlQualifiedName(definition.Name.LocalName, definition.Name.NamespaceName);
            _additionalTypeDefinitions.Add(type, qualifiedName);

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
            _portType.Operations.Add(new Operation
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

            _addGlobalNamespace(NamespaceConstants.Mime);

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

            operationBinding.Extensions.Add(_schemaDefinitionProvider.ProtocolDefinition.Style.CreateSoapOperationBinding(operationDefinition.SoapAction));

            _binding.Operations.Add(operationBinding);
        }

        private void AddOperationContentBinding<THeader>(ServiceDescriptionFormatExtensionCollection extensions, Func<SoapHeaderBinding, THeader> projectionFunc)
            where THeader : ServiceDescriptionFormatExtension
        {
            extensions.Add(_schemaDefinitionProvider.ProtocolDefinition.Style.CreateSoapBodyBinding(_schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace));
            foreach (var headerBinding in _headerDefinition.RequiredHeaders.Select(name => _schemaDefinitionProvider.ProtocolDefinition.Style.CreateSoapHeaderBinding(name, _headerDefinition.MessageName, _schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace)).Select(projectionFunc))
                extensions.Add(headerBinding);
        }

        private XmlQualifiedName AddComplexTypeContent(XmlSchemaComplexType schemaType, TypeDefinition typeDefinition, SchemaReferences schemaReferences)
        {
            var contentParticle = new XmlSchemaSequence();

            var propertyDefinitions = GetDescriptionProperties(typeDefinition).ToList();

            foreach (var propertyDefinition in propertyDefinitions)
            {
                var propertyContent = propertyDefinition.Content;

                if (propertyContent.MergeContent && propertyDefinitions.Count > 1 && propertyContent is SingularContentDefinition)
                    throw new SchemaDefinitionException($"Property {propertyDefinition} of type {typeDefinition} cannot be merged, because there are more than 1 properties present.");

                var contentElements = CreateContentElement(propertyContent, schemaReferences);

                if (!propertyContent.MergeContent || propertyContent is ArrayContentDefiniton)
                {
                    contentParticle.Items.Add(contentElements.Item2);
                    continue;
                }

                if (!contentElements.Item1.SchemaTypeName.IsEmpty)
                    return contentElements.Item1.SchemaTypeName;

                var particle = ((XmlSchemaComplexType)contentElements.Item1.SchemaType)?.Particle;
                if (particle != null)
                    schemaType.Particle = particle;

                var content = ((XmlSchemaComplexType)contentElements.Item1.SchemaType)?.ContentModel;
                if (content != null)
                    schemaType.ContentModel = content;

                return null;
            }

            if (!typeDefinition.Type.HasBaseType())
                schemaType.Particle = contentParticle;
            else
            {
                var extension = new XmlSchemaComplexContentExtension
                {
                    BaseTypeName = GetSchemaTypeName(typeDefinition.Type.BaseType, schemaReferences.SchemaNamespace),
                    Particle = contentParticle
                };

                if (!schemaReferences.Types.ContainsKey(extension.BaseTypeName))
                    schemaReferences.Types.Add(extension.BaseTypeName, null);

                schemaType.ContentModel = new XmlSchemaComplexContent { Content = extension };
            }

            return null;
        }

        private XmlQualifiedName GetSchemaTypeName(Type type, string schemaNamespace)
        {
            var name = type.GetSystemTypeName();
            if (name != null)
                return new XmlQualifiedName(name.LocalName, name.NamespaceName);

            if (!_runtimeTypeDefinitions.TryGetValue(type, out var typeDefinition))
                throw new SchemaDefinitionException($"Unrecognized type `{type.FullName}`.");

            _addRequiredImport(schemaNamespace, typeDefinition.Name.NamespaceName, null);

            return new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
        }

        private IList<XRoadTitleAttribute> GetTitlesOrDefault(DocumentationDefinition documentationDefinition, string defaultTitle)
        {
            if (documentationDefinition.Titles.Any())
                return documentationDefinition.Titles;

            var titles = new List<XRoadTitleAttribute>();

            if (_schemaDefinitionProvider.ProtocolDefinition.GenerateFakeXRoadDocumentation)
                titles.Add(new XRoadTitleAttribute(defaultTitle));

            return titles;
        }

        private XmlSchemaAnnotation CreateSchemaAnnotation(string schemaNamespace, DocumentationDefinition documentationDefinition, string defaultTitle)
        {
            if (documentationDefinition.IsEmpty && (!_schemaDefinitionProvider.ProtocolDefinition.GenerateFakeXRoadDocumentation || string.IsNullOrWhiteSpace(defaultTitle)))
                return null;

            var markup =
                GetTitlesOrDefault(documentationDefinition, defaultTitle)
                    .Select(doc => CreateXrdDocumentationElement("title", doc.LanguageCode, doc.Value, ns => _addRequiredImport(schemaNamespace, ns, null)))
                    .Concat(documentationDefinition
                            .Notes
                            .Select(doc => CreateXrdDocumentationElement("notes", doc.LanguageCode, doc.Value, ns => _addRequiredImport(schemaNamespace, ns, null))))
                    .Concat(documentationDefinition
                            .TechNotes
                            .Select(doc => CreateXrdDocumentationElement(_schemaDefinitionProvider.ProtocolDefinition.TechNotesElementName, doc.LanguageCode, doc.Value, ns => _addRequiredImport(schemaNamespace, ns, null))))
                    .Cast<XmlNode>();

            var appInfo = new XmlSchemaAppInfo { Markup = markup.ToArray() };

            return new XmlSchemaAnnotation { Items = { appInfo } };
        }

        private void AddBinaryAttribute(string schemaNamespace, XmlSchemaAnnotated schemaElement)
        {
            _addRequiredImport(schemaNamespace, NamespaceConstants.Xmime, null);

            schemaElement.UnhandledAttributes = new[] { _schemaDefinitionProvider.ProtocolDefinition.Style.CreateExpectedContentType("application/octet-stream") };
        }

        private TypeDefinition GetContentTypeDefinition(ContentDefinition contentDefinition)
        {
            if (contentDefinition.TypeName != null)
                return _schemaTypeDefinitions[contentDefinition.TypeName];

            if (_runtimeTypeDefinitions.ContainsKey(contentDefinition.RuntimeType))
                return _runtimeTypeDefinitions[contentDefinition.RuntimeType];

            return _schemaDefinitionProvider.GetTypeDefinition(contentDefinition.RuntimeType);
        }

        private void SetSchemaElementType(XmlSchemaElement schemaElement, ContentDefinition contentDefinition, SchemaReferences schemaReferences)
        {
            if (typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType) && contentDefinition.UseXop)
                AddBinaryAttribute(schemaReferences.SchemaNamespace, schemaElement);

            var typeDefinition = GetContentTypeDefinition(contentDefinition);
            if (!typeDefinition.IsAnonymous)
            {
                if (contentDefinition.RuntimeType == typeof(object))
                    return;

                _addRequiredImport(schemaReferences.SchemaNamespace, typeDefinition.Name.NamespaceName, null);
                schemaElement.SchemaTypeName = new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);

                if (!schemaReferences.Types.ContainsKey(schemaElement.SchemaTypeName))
                    schemaReferences.Types.Add(schemaElement.SchemaTypeName, null);

                return;
            }

            XmlSchemaType schemaType;
            XmlQualifiedName schemaTypeName = null;

            if (contentDefinition.RuntimeType.IsEnum)
            {
                schemaType = new XmlSchemaSimpleType();
                AddEnumTypeContent(schemaReferences.SchemaNamespace, contentDefinition.RuntimeType, (XmlSchemaSimpleType)schemaType);
            }
            else
            {
                schemaType = new XmlSchemaComplexType();
                schemaTypeName = AddComplexTypeContent((XmlSchemaComplexType)schemaType, typeDefinition, schemaReferences);
            }

            schemaType.Annotation = CreateSchemaAnnotation(schemaReferences.SchemaNamespace, typeDefinition.Documentation, schemaTypeName?.Name ?? contentDefinition.RuntimeType.Name);

            if (schemaTypeName == null)
                schemaElement.SchemaType = schemaType;
            else schemaElement.SchemaTypeName = schemaTypeName;
        }

        private void AddEnumTypeContent(string schemaNamespace, Type type, XmlSchemaSimpleType schemaType)
        {
            var restriction = new XmlSchemaSimpleTypeRestriction { BaseTypeName = new XmlQualifiedName("string", NamespaceConstants.Xsd) };

            foreach (var name in Enum.GetNames(type))
            {
                var memberInfo = type.GetMember(name).Single();
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

        private Tuple<XmlSchemaElement, XmlSchemaElement> CreateContentElement(ContentDefinition contentDefinition, SchemaReferences schemaReferences)
        {
            var schemaElements = CreateXmlSchemaElement(contentDefinition.Name, schemaReferences);

            var arrayContentDefinition = contentDefinition as ArrayContentDefiniton;

            if (arrayContentDefinition != null && contentDefinition.MergeContent)
            {
                schemaElements = SetXmlSchemaElementName(schemaElements, arrayContentDefinition.Item.Content.Name, schemaReferences);

                schemaElements.Item1.Annotation = CreateSchemaAnnotation(schemaReferences.SchemaNamespace, contentDefinition.Documentation, schemaElements.Item1.Name);

                if (arrayContentDefinition.Item.MinOccurs != 1u)
                    schemaElements.Item2.MinOccurs = arrayContentDefinition.Item.MinOccurs;
                
                if (!arrayContentDefinition.Item.MaxOccurs.HasValue)
                    schemaElements.Item2.MaxOccursString = "unbounded";
                else if (arrayContentDefinition.Item.MaxOccurs.Value != 1u)
                    schemaElements.Item2.MaxOccurs = arrayContentDefinition.Item.MaxOccurs.Value;

                schemaElements.Item1.IsNillable = arrayContentDefinition.Item.Content.IsNullable;

                SetSchemaElementType(schemaElements.Item1, arrayContentDefinition.Item.Content, schemaReferences);

                return schemaElements;
            }

            schemaElements.Item1.Annotation = CreateSchemaAnnotation(schemaReferences.SchemaNamespace, contentDefinition.Documentation, schemaElements.Item1.Name);

            if (contentDefinition.IsOptional)
                schemaElements.Item2.MinOccurs = 0;

            schemaElements.Item1.IsNillable = contentDefinition.IsNullable;

            if (arrayContentDefinition == null)
            {
                SetSchemaElementType(schemaElements.Item1, contentDefinition, schemaReferences);
                return schemaElements;
            }

            if (contentDefinition.TypeName != null)
            {
                var typeDefinition = _schemaTypeDefinitions[contentDefinition.TypeName];
                schemaElements.Item1.SchemaTypeName = new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
                if (schemaReferences.Types.ContainsKey(schemaElements.Item1.SchemaTypeName))
                    schemaReferences.Types.Add(schemaElements.Item1.SchemaTypeName, null);
                return schemaElements;
            }

            var itemElements = CreateXmlSchemaElement(arrayContentDefinition.Item.Content.Name, schemaReferences);

            if (arrayContentDefinition.Item.MinOccurs != 1)
                itemElements.Item2.MinOccurs = arrayContentDefinition.Item.MinOccurs;
                
            if (!arrayContentDefinition.Item.MaxOccurs.HasValue)
                itemElements.Item2.MaxOccursString = "unbounded";
            else if (arrayContentDefinition.Item.MaxOccurs.Value != 1u)
                itemElements.Item2.MaxOccurs = arrayContentDefinition.Item.MaxOccurs.Value;

            itemElements.Item1.IsNillable = arrayContentDefinition.Item.Content.IsNullable;

            SetSchemaElementType(itemElements.Item1, arrayContentDefinition.Item.Content, schemaReferences);

            _schemaDefinitionProvider.ProtocolDefinition.Style.AddItemElementToArrayElement(schemaElements.Item1, itemElements.Item2, ns => _addRequiredImport(schemaReferences.SchemaNamespace, ns, null));

            return schemaElements;
        }

        public ServiceDescription GetServiceDescription()
        {
            var protocolDefinition = _schemaDefinitionProvider.ProtocolDefinition;
            var serviceDescription = new ServiceDescription { TargetNamespace = protocolDefinition.ProducerNamespace };

            var standardHeader = new Message { Name = _headerDefinition.MessageName };

            foreach (var requiredHeader in _headerDefinition.RequiredHeaders)
            {
                standardHeader.Parts.Add(new MessagePart { Name = requiredHeader.LocalName, Element = new XmlQualifiedName(requiredHeader.LocalName, requiredHeader.NamespaceName) });
                _addRequiredImport(serviceDescription.TargetNamespace, requiredHeader.NamespaceName, null);
            }

            serviceDescription.Messages.Add(standardHeader);

            foreach (var schema in BuildSchemas(protocolDefinition.ProducerNamespace, serviceDescription.Messages))
                serviceDescription.Types.Schemas.Add(schema);

            serviceDescription.PortTypes.Add(_portType);

            _binding.Extensions.Add(protocolDefinition.Style.CreateSoapBinding());
            serviceDescription.Bindings.Add(_binding);

            _servicePort.Extensions.Add(new SoapAddressBinding { Location = protocolDefinition.SoapAddressLocation });

            serviceDescription.Services.Add(_service);

            AddServiceDescriptionNamespaces(serviceDescription);

            _schemaDefinitionProvider.ExportServiceDescription(serviceDescription);

            return serviceDescription;
        }

        private void AddServiceDescriptionNamespaces(DocumentableItem serviceDescription)
        {
            foreach (var tuple in _getGlobalNamespaces())
                serviceDescription.Namespaces.Add(tuple.Item1, tuple.Item2);
            serviceDescription.Namespaces.Add(PrefixConstants.Target, _schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace);
        }

        private IEnumerable<PropertyDefinition> GetDescriptionProperties(TypeDefinition typeDefinition)
        {
            return typeDefinition.Type.GetPropertiesSorted(typeDefinition.ContentComparer, _version, p => _schemaDefinitionProvider.GetPropertyDefinition(p, typeDefinition)).Where(d => d.Content.State == DefinitionState.Default);
        }

        private void AddSystemType<T>(string typeName)
        {
            var typeDefinition = _schemaDefinitionProvider.GetSimpleTypeDefinition<T>(typeName);

            if (typeDefinition.Type != null && !_runtimeTypeDefinitions.ContainsKey(typeDefinition.Type))
                _runtimeTypeDefinitions.Add(typeDefinition.Type, typeDefinition);

            if (typeDefinition.Name != null && !_schemaTypeDefinitions.ContainsKey(typeDefinition.Name))
                _schemaTypeDefinitions.Add(typeDefinition.Name, typeDefinition);
        }

        private void AddSchemaType(ICollection<Tuple<string, XmlSchemaType>> schemaTypes, string namespaceName, XmlSchemaType schemaType)
        {
            if (!_schemaLocations.ContainsKey(namespaceName))
                _schemaLocations.Add(namespaceName, _schemaDefinitionProvider.GetSchemaLocation(namespaceName));

            schemaTypes.Add(Tuple.Create(namespaceName, schemaType));
        }

        private XRoadOperationVersionBinding CreateXRoadOperationVersionBinding(OperationDefinition operationDefinition)
        {
            return operationDefinition.Version == 0 ?
                null :
                new XRoadOperationVersionBinding(_xRoadPrefix, _xRoadNamespace) { Version = $"v{operationDefinition.Version}" };
        }

        private XmlElement CreateXrdDocumentationElement(string name, string languageCode, string value, Action<string> addSchemaImport)
        {
            addSchemaImport(_xRoadNamespace);

            var titleElement = _document.CreateElement(_xRoadPrefix, name, _xRoadNamespace);
            titleElement.InnerText = value;

            if (string.IsNullOrWhiteSpace(languageCode))
                return titleElement;

            var attribute = _document.CreateAttribute("xml", "lang", null);
            attribute.Value = languageCode;
            titleElement.Attributes.Append(attribute);

            return titleElement;
        }

        private XmlElement CreateDocumentationElement(DocumentationDefinition documentationDefinition, string defaultTitle)
        {
            if (documentationDefinition.IsEmpty && (!_schemaDefinitionProvider.ProtocolDefinition.GenerateFakeXRoadDocumentation || string.IsNullOrWhiteSpace(defaultTitle)))
                return null;

            var documentationElement = _document.CreateElement(PrefixConstants.Wsdl, "documentation", NamespaceConstants.Wsdl);

            foreach (var title in GetTitlesOrDefault(documentationDefinition, defaultTitle))
                documentationElement.AppendChild(CreateXrdDocumentationElement("title", title.LanguageCode, title.Value, _ => { }));

            foreach (var title in documentationDefinition.Notes)
                documentationElement.AppendChild(CreateXrdDocumentationElement("notes", title.LanguageCode, title.Value, _ => { }));

            foreach (var title in documentationDefinition.TechNotes)
                documentationElement.AppendChild(CreateXrdDocumentationElement(_schemaDefinitionProvider.ProtocolDefinition.TechNotesElementName, title.LanguageCode, title.Value, _ => { }));

            return documentationElement;
        }

        private XmlAttribute CreateAttribute(XName attributeName, string value, Action<string> addSchemaImport)
        {
            addSchemaImport(attributeName.NamespaceName);

            var attribute = _document.CreateAttribute(attributeName.LocalName, attributeName.NamespaceName);
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

        private Tuple<XmlSchemaElement, XmlSchemaElement> CreateXmlSchemaElement(XName name, SchemaReferences schemaReferences)
        {
            var elementName = name?.LocalName ?? string.Empty;
            var elementNamespace = name?.NamespaceName ?? string.Empty;

            var element = new XmlSchemaElement { Name = elementName };
            var referenceElement = element;

            var ns = schemaReferences.SchemaNamespace;
            if (elementNamespace != schemaReferences.SchemaNamespace && elementNamespace != "")
            {
                ns = elementNamespace;
                referenceElement = new XmlSchemaElement { RefName = new XmlQualifiedName(elementName, elementNamespace) };
                schemaReferences.Elements.Add(referenceElement.RefName, element);
            }

            var elementForm =
                _schemaDefinitionProvider.IsQualifiedElementDefault(ns)
                    ? (elementNamespace == "" ? (XmlSchemaForm?)XmlSchemaForm.Unqualified : null)
                    : (elementNamespace == ns ? (XmlSchemaForm?)XmlSchemaForm.Qualified : null);

            if (elementForm.HasValue)
                element.Form = elementForm.Value;

            return Tuple.Create(element, referenceElement);
        }

        private Tuple<XmlSchemaElement, XmlSchemaElement> SetXmlSchemaElementName(Tuple<XmlSchemaElement, XmlSchemaElement> elements, XName name, SchemaReferences schemaReferences)
        {
            var element = elements.Item1;
            var referenceElement = elements.Item2;

            var ns = schemaReferences.SchemaNamespace;
            if (name.NamespaceName != schemaReferences.SchemaNamespace && name.NamespaceName != "")
            {
                ns = name.NamespaceName;
                referenceElement = new XmlSchemaElement { RefName = new XmlQualifiedName(name.LocalName, name.NamespaceName) };
                schemaReferences.Elements.Add(referenceElement.RefName, element);
            }

            element.Name = name.LocalName;

            var elementForm =
                _schemaDefinitionProvider.IsQualifiedElementDefault(ns)
                    ? (name.NamespaceName == "" ? (XmlSchemaForm?)XmlSchemaForm.Unqualified : null)
                    : (name.NamespaceName == ns ? (XmlSchemaForm?)XmlSchemaForm.Qualified : null);

            if (elementForm.HasValue)
                element.Form = elementForm.Value;

            return Tuple.Create(element, referenceElement);
        }

        private class SchemaReferences
        {
            public string SchemaNamespace { get; }

            public IDictionary<XmlQualifiedName, XmlSchemaElement> Elements { get; } =
                new Dictionary<XmlQualifiedName, XmlSchemaElement>();

            public IDictionary<XmlQualifiedName, XmlSchemaType> Types { get; } =
                new Dictionary<XmlQualifiedName, XmlSchemaType>();

            public SchemaReferences(string schemaNamespace)
            {
                SchemaNamespace = schemaNamespace;
            }
        }
    }
}