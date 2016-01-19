using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Attributes;
using XRoadLib.Configuration;
using XRoadLib.Extensions;
using XRoadLib.Header;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public sealed class ProducerDefinition
    {
        private const string STANDARD_HEADER_NAME = "stdhdr";
        private const string DEFAULT_LOCATION = "http://TURVASERVER/cgi-bin/consumer_proxy";

        private readonly XmlDocument document = new XmlDocument();

        private readonly Assembly contractAssembly;
        private readonly XRoadProtocol protocol;
        private readonly string environmentProducerName;
        private readonly string xroadNamespace;
        private readonly string targetNamespace;
        private readonly string standardHeaderName;
        private readonly uint? version;
        private readonly IOperationConfiguration operationConfiguration;
        private readonly ITypeConfiguration typeConfiguration;

        private readonly string requestTypeNameFormat;
        private readonly string responseTypeNameFormat;
        private readonly string requestMessageNameFormat;
        private readonly string responseMessageNameFormat;

        private readonly Binding binding;
        private readonly Port servicePort;
        private readonly PortType portType;
        private readonly Service service;

        private readonly IDictionary<MethodInfo, IDictionary<string, XRoadServiceAttribute>> serviceContracts;

        private readonly ICollection<string> requiredHeaders = new SortedSet<string>();
        private readonly IList<Message> messages = new List<Message>();
        private readonly IList<XmlSchemaImport> schemaImports = new List<XmlSchemaImport>();
        private readonly IDictionary<string, XmlSchemaElement> schemaElements = new SortedDictionary<string, XmlSchemaElement>();
        private readonly IDictionary<string, Tuple<Type, XmlSchemaComplexType>> schemaTypes = new SortedDictionary<string, Tuple<Type, XmlSchemaComplexType>>();
        private readonly IDictionary<string, Tuple<MethodInfo, XmlSchemaComplexType, XmlSchemaComplexType>> operationTypes = new SortedDictionary<string, Tuple<MethodInfo, XmlSchemaComplexType, XmlSchemaComplexType>>();

        public string HeaderMessage { private get; set; }
        public string Location { private get; set; }

        public ProducerDefinition(Assembly contractAssembly, XRoadProtocol protocol, uint? version = null, string environmentProducerName = null)
        {
            if (contractAssembly == null)
                throw new ArgumentNullException(nameof(contractAssembly));
            this.contractAssembly = contractAssembly;

            if (!protocol.HasDefinedValue())
                throw new ArgumentException($"Only defined X-Road protocol values are allowed, but was `{protocol}`.", nameof(protocol));
            this.protocol = protocol;

            var producerConfiguration = protocol.GetContractConfiguration(contractAssembly);
            var producerName = contractAssembly.GetProducerName();

            this.environmentProducerName = environmentProducerName.GetValueOrDefault(producerName);
            this.version = version;

            xroadNamespace = protocol.GetNamespace();
            targetNamespace = protocol.GetProducerNamespace(producerName);

            portType = new PortType
            {
                Name = producerConfiguration.PortTypeName.GetValueOrDefault($"{producerName}PortType")
            };

            binding = new Binding
            {
                Name = producerConfiguration.BindingName.GetValueOrDefault($"{producerName}Binding"),
                Type = new XmlQualifiedName(portType.Name, targetNamespace)
            };

            servicePort = new Port
            {
                Name = producerConfiguration.ServicePortName.GetValueOrDefault($"{producerName}Port"),
                Binding = new XmlQualifiedName(binding.Name, targetNamespace),
                Extensions = { CreateAddressBindingElement() }
            };

            service = new Service
            {
                Name = producerConfiguration.ServiceName.GetValueOrDefault($"{producerName}Service"),
                Ports = { servicePort }
            };

            standardHeaderName = producerConfiguration.StandardHeaderName.GetValueOrDefault(STANDARD_HEADER_NAME);
            requestTypeNameFormat = producerConfiguration.RequestTypeNameFormat.GetValueOrDefault("{0}");
            responseTypeNameFormat = producerConfiguration.ResponseTypeNameFormat.GetValueOrDefault("{0}Response");
            requestMessageNameFormat = producerConfiguration.RequestMessageNameFormat.GetValueOrDefault("{0}");
            responseMessageNameFormat = producerConfiguration.ResponseMessageNameFormat.GetValueOrDefault("{0}Response");
            operationConfiguration = producerConfiguration.OperationConfiguration;
            typeConfiguration = producerConfiguration.TypeConfiguration;

            serviceContracts = contractAssembly.GetServiceContracts();

            AddTypes();
            AddServiceContracts();
        }

        public void SaveTo(Stream stream)
        {
            var startTime = DateTime.Now;
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n" }))
            {
                writer.WriteStartDocument();

                writer.WriteComment($" WSDL document generated by {GetType().FullName} ");
                writer.WriteComment($" WSDL document generated at {startTime:dd.MM.yyyy HH:mm:ss} ");
                writer.WriteComment($" {HeaderMessage} ");

                WriteServiceDescription(writer);
                writer.Flush();

                writer.WriteComment($" WSDL document generated in {(DateTime.Now - startTime).TotalMilliseconds} ms. ");

                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        public void AddOperation(string name, MethodInfo methodImpl, bool isExported)
        {
            var methodContract = methodImpl.FindMethodDeclaration(name, serviceContracts);
            if (methodContract.Item2.IsHidden || (version.HasValue && !methodContract.Item2.IsDefinedInVersion(version.Value)))
                return;

            BuildOperationElements(name, methodContract.Item1, isExported);

            if (isExported)
                return;

            var serviceVersion = version.GetValueOrDefault(methodContract.Item2.AddedInVersion);

            var operationBinding = new OperationBinding
            {
                Name = name,
                Extensions = { CreateXRoadVersionBindingElement(serviceVersion) },
                Input = new InputBinding(),
                Output = new OutputBinding()
            };

            BuildOperationBinding(operationBinding, methodContract.Item1);

            binding.Operations.Add(operationBinding);
        }

        public ProducerDefinition AddHeader<T>(Expression<Func<IXRoadHeader, T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Only MemberExpression is allowed to use for SOAP header definition, but was {expression.Body.GetType().Name} ({GetType().Name}).", nameof(expression));

            var elementName = memberExpression.Member.GetElementName();
            if (string.IsNullOrWhiteSpace(elementName))
                throw new ArgumentException($"Specified member `{memberExpression.Member.Name}` does not define any XML element.", nameof(expression));

            requiredHeaders.Add(elementName);

            return this;
        }

        private void WriteServiceDescription(XmlWriter writer)
        {
            var serviceDescription = new ServiceDescription { TargetNamespace = targetNamespace };
            AddServiceDescriptionNamespaces(serviceDescription);

            var schema = new XmlSchema { TargetNamespace = targetNamespace };
            CreateXmlSchemaImports(schema);

            foreach (var schemaType in schemaTypes)
                schema.Items.Add(schemaType.Value.Item2);

            AddOperationTypes(schema);
            AddSchemaElements(schema);

            serviceDescription.Types.Schemas.Add(schema);

            serviceDescription.PortTypes.Add(portType);

            AddSoapBinding();
            serviceDescription.Bindings.Add(binding);

            var standardHeader = new Message { Name = standardHeaderName };

            foreach (var requiredHeader in requiredHeaders)
                standardHeader.Parts.Add(new MessagePart { Name = requiredHeader, Element = new XmlQualifiedName(requiredHeader, xroadNamespace) });

            serviceDescription.Messages.Add(standardHeader);

            foreach (var message in messages)
                serviceDescription.Messages.Add(message);

            foreach (var title in contractAssembly.GetXRoadTitles().OrderBy(t => t.Item1.ToLower()))
                servicePort.Extensions.Add(CreateXRoadTitleElement(title.Item1, title.Item2));

            servicePort.Extensions.Add(new SoapAddressBinding
            {
                Location = string.IsNullOrWhiteSpace(Location) ? DEFAULT_LOCATION : Location
            });

            serviceDescription.Services.Add(service);

            serviceDescription.Write(writer);
        }

        private void AddTypes()
        {
            foreach (var type in contractAssembly.GetTypes().Where(type => type.IsXRoadSerializable() && (!version.HasValue || type.ExistsInVersion(version.Value))))
            {
                if (IsExistingType(type.Name))
                    throw new Exception($"Multiple type definitions for same name `{type.Name}`.");

                var schemaType = new XmlSchemaComplexType { Name = type.Name, IsAbstract = type.IsAbstract, Annotation = CreateAnnotationElement(type) };
                schemaTypes.Add(type.Name, Tuple.Create(type, schemaType));
            }

            foreach (var value in schemaTypes.Values)
            {
                var type = value.Item1;
                var schemaType = value.Item2;
                var properties = type.GetPropertiesSorted(typeConfiguration?.GetPropertyComparer(type) ?? DefaultComparer.Instance, version);

                var contentParticle = new XmlSchemaSequence();

                foreach (var property in properties)
                {
                    var propertyElement = new XmlSchemaElement { Name = property.GetPropertyName(), Annotation = CreateAnnotationElement(property) };
                    AddSchemaType(propertyElement, property.PropertyType, true, property.GetQualifiedTypeName());
                    contentParticle.Items.Add(propertyElement);
                }

                if (type.BaseType != typeof(XRoadSerializable))
                {
                    var extension = new XmlSchemaComplexContentExtension { BaseTypeName = GetComplexTypeName(type.BaseType), Particle = contentParticle };
                    var complexContent = new XmlSchemaComplexContent { Content = extension };
                    schemaType.ContentModel = complexContent;
                }
                else schemaType.Particle = contentParticle;
            }
        }

        private void AddServiceContracts()
        {
            AddMessageTypes(
                serviceContracts.Select(kvp => Tuple.Create(kvp.Key,
                    kvp.Value
                        .Where(v => !v.Value.IsHidden && (!version.HasValue || v.Value.IsDefinedInVersion(version.Value)))
                        .ToDictionary(y => y.Key, y => y.Value)))
                    .Where(x => x.Item2.Any())
                    .ToDictionary(x => x.Item1, x => x.Item2.Keys.ToList()));
        }

        private void AddSchemaType(XmlSchemaElement schemaElement, Type runtimeType, bool isOptional, XName qualifiedTypeName)
        {
            var element = schemaElement;
            var type = runtimeType;

            if (isOptional)
                element.MinOccurs = 0;

            while (type.IsArray)
            {
                element.IsNillable = true;

                if (type.GetArrayRank() > 1)
                    throw new NotImplementedException("Multi-dimensional arrays are not supported for DTO types.");

                var itemElement = new XmlSchemaElement { Name = "item", MinOccurs = 0, MaxOccursString = "unbounded" };

                type = type.GetElementType();

                CreateArrayDefinition(element, itemElement, type);

                element = itemElement;
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                type = nullableType;
                element.IsNillable = true;
            }

            if (type.IsClass)
                element.IsNillable = true;

            if (type == typeof(Stream))
                AddBinaryAttribute(element);

            if (qualifiedTypeName != null)
            {
                element.SchemaTypeName = new XmlQualifiedName(qualifiedTypeName.LocalName.Equals("base64") ? "base64Binary" : qualifiedTypeName.LocalName, NamespaceConstants.XSD);
                return;
            }

            var qualifiedName = GetSimpleTypeName(type);
            if (qualifiedName != null)
            {
                element.SchemaTypeName = qualifiedName;
                return;
            }

            element.IsNillable = true;
            element.SchemaTypeName = GetComplexTypeName(type);
        }

        private XmlQualifiedName GetSimpleTypeName(Type type)
        {
            switch (type.FullName)
            {
                case "System.Boolean":
                    return new XmlQualifiedName("boolean", NamespaceConstants.XSD);
                case "System.DateTime":
                    return new XmlQualifiedName("dateTime", NamespaceConstants.XSD);
                case "System.Decimal":
                    return new XmlQualifiedName("decimal", NamespaceConstants.XSD);
                case "System.Int32":
                    return new XmlQualifiedName("int", NamespaceConstants.XSD);
                case "System.Int64":
                    return new XmlQualifiedName("long", NamespaceConstants.XSD);
                case "System.String":
                    return new XmlQualifiedName("string", NamespaceConstants.XSD);
                case "System.IO.Stream":
                    return new XmlQualifiedName("base64Binary", GetBinaryNamespace());
                default:
                    return null;
            }
        }

        private XmlQualifiedName GetComplexTypeName(Type type)
        {
            Tuple<Type, XmlSchemaComplexType> value;
            if (!schemaTypes.TryGetValue(type.Name, out value) || value.Item1 != type)
                throw new Exception($"Unrecognized type `{type.FullName}`");

            return new XmlQualifiedName(type.Name, targetNamespace);
        }

        private void BuildOperationElements(string name, MethodInfo methodContract, bool isExported)
        {
            var importAttribute = methodContract.GetImportAttribute(protocol);
            if (importAttribute != null)
            {
                BuildImportedOperationElements(name, methodContract, importAttribute);
                return;
            }

            var operationTypeName = GetOperationTypeName(name, methodContract);

            var requestElement = new XmlSchemaElement
            {
                Name = name,
                SchemaTypeName = operationTypeName.Item1
            };

            var responseElement = new XmlSchemaElement
            {
                Name = $"{name}Response",
                SchemaTypeName = operationTypeName.Item2
            };

            schemaElements.Add(requestElement.Name, requestElement);
            schemaElements.Add(responseElement.Name, responseElement);

            var inputMessage = CreateOperationMessage(requestElement, methodContract);
            var outputMessage = CreateOperationMessage(responseElement, methodContract, inputMessage);

            if (!isExported)
            {
                var operation = new Operation { Name = name, DocumentationElement = CreateDocumentationElement(methodContract) };

                operation.Messages.Add(new OperationInput { Message = new XmlQualifiedName(inputMessage.Name, targetNamespace) });
                operation.Messages.Add(new OperationOutput { Message = new XmlQualifiedName(outputMessage.Name, targetNamespace) });

                portType.Operations.Add(operation);
            }

            messages.Add(inputMessage);
            messages.Add(outputMessage);
        }

        private static string GetOperationNameFromMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType == null)
                throw new ArgumentException("Declaring type is missing.", nameof(methodInfo));

            if (methodInfo.DeclaringType.Name.StartsWith("I") && methodInfo.DeclaringType.Name.Length > 1 && char.IsUpper(methodInfo.DeclaringType.Name[1]))
                return methodInfo.DeclaringType.Name.Substring(1);

            return methodInfo.DeclaringType.Name;
        }

        private void AddMessageType(string operationName, MethodInfo method)
        {
            var importAttribute = method.GetImportAttribute(protocol);
            if (importAttribute != null)
            {
                var schemaImportProvider = (ISchemaImportProvider)Activator.CreateInstance(importAttribute.SchemaImportProvider);
                schemaImports.Add(new XmlSchemaImport
                {
                    SchemaLocation = schemaImportProvider.SchemaLocation,
                    Namespace = schemaImportProvider.SchemaNamespace
                });
                return;
            }

            var requestName = string.Format(requestTypeNameFormat, operationName);
            var responseName = string.Format(responseTypeNameFormat, operationName);

            if (IsExistingType(requestName) || IsExistingType(responseName))
                throw new Exception($"Operation type `{requestName}` already exists with the same name.");

            var requestSequence = CreateOperationRequestSequence(method, operationName);
            var requestType = new XmlSchemaComplexType { Name = requestName, Particle = requestSequence, Annotation = CreateAnnotationElement(method) };

            var responseType = CreateResponseType(responseName, method, requestSequence, operationName);

            operationTypes.Add(requestName, Tuple.Create(method, requestType, responseType));
        }

        private bool IsExistingType(string typeName)
        {
            return schemaTypes.ContainsKey(typeName) || operationTypes.ContainsKey(typeName);
        }

        private XmlAttribute CreateAttribute(string prefix, string name, string @namespace, string value)
        {
            var attribute = document.CreateAttribute(prefix, name, @namespace);
            attribute.Value = value;
            return attribute;
        }

        private XmlElement CreateElement(string prefix, string name, string @namespace)
        {
            return document.CreateElement(prefix, name, @namespace);
        }

        #region Contract definitions that depend on X-Road protocol version

        private void BuildImportedOperationElements(string name, MethodInfo methodContract, XRoadImportAttribute importAttribute)
        {
            var importNamespace = ((ISchemaImportProvider)Activator.CreateInstance(importAttribute.SchemaImportProvider)).SchemaNamespace;

            var operationTypeName = Tuple.Create(new XmlQualifiedName(importAttribute.RequestPart, importNamespace),
                                                 new XmlQualifiedName(importAttribute.ResponsePart, importNamespace));

            var extraParts = methodContract.GetExtraMessageParts().ToList();

            var inputMessage = new Message
            {
                Name = string.Format(requestMessageNameFormat, name)
            };

            inputMessage.Parts.Add(
                protocol == XRoadProtocol.Version20
                    ? new MessagePart { Name = "keha", Type = operationTypeName.Item1 }
                    : new MessagePart { Name = "body", Element = operationTypeName.Item1 });

            var outputMessage = new Message
            {
                Name = string.Format(responseMessageNameFormat, name)
            };

            if (protocol == XRoadProtocol.Version20)
                outputMessage.Parts.Add(new MessagePart { Name = "paring", Type = operationTypeName.Item1 });

            outputMessage.Parts.Add(
                protocol == XRoadProtocol.Version20
                    ? new MessagePart { Name = "keha", Type = operationTypeName.Item2 }
                    : new MessagePart { Name = "body", Element = operationTypeName.Item2 });

            if (protocol == XRoadProtocol.Version20)
                foreach (var part in extraParts)
                {
                    var message = part.Direction == MessagePartDirection.Input ? inputMessage : outputMessage;
                    message.Parts.Add(new MessagePart { Name = part.PartName, Type = new XmlQualifiedName(part.TypeName, importNamespace) });
                }

            var operation = new Operation { Name = name, DocumentationElement = CreateDocumentationElement(methodContract) };

            operation.Messages.Add(new OperationInput { Message = new XmlQualifiedName(inputMessage.Name, targetNamespace) });
            operation.Messages.Add(new OperationOutput { Message = new XmlQualifiedName(outputMessage.Name, targetNamespace) });

            portType.Operations.Add(operation);

            messages.Add(inputMessage);
            messages.Add(outputMessage);
        }

        private void CreateArrayDefinition(XmlSchemaElement element, XmlSchemaObject itemElement, Type type)
        {
            if (protocol != XRoadProtocol.Version20)
            {
                element.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { itemElement } } };
                return;
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                type = nullableType;

            var qualifiedTypeName = GetSimpleTypeName(type) ?? GetComplexTypeName(type);

            var restriction = new XmlSchemaComplexContentRestriction
            {
                BaseTypeName = new XmlQualifiedName("Array", NamespaceConstants.SOAP_ENC),
                Particle = new XmlSchemaSequence { Items = { itemElement } },
                Attributes =
                {
                    new XmlSchemaAttribute
                    {
                        RefName = new XmlQualifiedName("arrayType", NamespaceConstants.SOAP_ENC),
                        UnhandledAttributes = new[]
                        {
                            CreateAttribute(PrefixConstants.WSDL, "arrayType", NamespaceConstants.WSDL, $"{qualifiedTypeName.Namespace}:{qualifiedTypeName.Name}[]")
                        }
                    }
                }
            };

            element.SchemaType = new XmlSchemaComplexType { ContentModel = new XmlSchemaComplexContent { Content = restriction } };
        }

        private void CreateXmlSchemaImports(XmlSchema schema)
        {
            schema.Includes.Add(new XmlSchemaImport { Namespace = xroadNamespace, SchemaLocation = xroadNamespace });

            if (protocol == XRoadProtocol.Version20)
                schema.Includes.Add(new XmlSchemaImport { Namespace = NamespaceConstants.SOAP_ENC, SchemaLocation = NamespaceConstants.SOAP_ENC });

            foreach (var import in schemaImports)
                schema.Includes.Add(import);
        }

        private void AddServiceDescriptionNamespaces(DocumentableItem serviceDescription)
        {
            serviceDescription.Namespaces.Add(PrefixConstants.MIME, NamespaceConstants.MIME);
            serviceDescription.Namespaces.Add(PrefixConstants.SOAP, NamespaceConstants.SOAP);
            serviceDescription.Namespaces.Add(PrefixConstants.SOAP_ENV, NamespaceConstants.SOAP_ENV);
            serviceDescription.Namespaces.Add(PrefixConstants.WSDL, NamespaceConstants.WSDL);
            serviceDescription.Namespaces.Add(PrefixConstants.XMIME, NamespaceConstants.XMIME);
            serviceDescription.Namespaces.Add(protocol.GetPrefix(), xroadNamespace);
            serviceDescription.Namespaces.Add(PrefixConstants.XSD, NamespaceConstants.XSD);
            serviceDescription.Namespaces.Add("", targetNamespace);

            if (protocol == XRoadProtocol.Version20)
                serviceDescription.Namespaces.Add(PrefixConstants.SOAP_ENC, NamespaceConstants.SOAP_ENC);
        }

        private void AddSoapBinding()
        {
            binding.Extensions.Add(new SoapBinding
            {
                Style = protocol == XRoadProtocol.Version20 ? SoapBindingStyle.Rpc : SoapBindingStyle.Document,
                Transport = NamespaceConstants.HTTP
            });
        }

        private void AddSoapOperationBinding(DocumentableItem operationBinding)
        {
            operationBinding.Extensions.Add(new SoapOperationBinding
            {
                Style = protocol == XRoadProtocol.Version20 ? SoapBindingStyle.Rpc : SoapBindingStyle.Document,
                SoapAction = ""
            });
        }

        private SoapBodyBinding CreateSoapBodyBinding()
        {
            return protocol == XRoadProtocol.Version20
                ? new SoapBodyBinding { Use = SoapBindingUse.Encoded, Namespace = targetNamespace, Encoding = NamespaceConstants.SOAP_ENC }
                : new SoapBodyBinding { Use = SoapBindingUse.Literal };
        }

        private SoapHeaderBinding CreateSoapHeaderBinding(string headerName)
        {
            return new SoapHeaderBinding
            {
                Message = new XmlQualifiedName(standardHeaderName, targetNamespace),
                Part = headerName,
                Use = (protocol == XRoadProtocol.Version20 ? SoapBindingUse.Encoded : SoapBindingUse.Literal),
                Namespace = (protocol == XRoadProtocol.Version20 ? xroadNamespace : null),
                Encoding = (protocol == XRoadProtocol.Version20 ? NamespaceConstants.SOAP_ENC : null)
            };
        }

        private MimePart BuildMultipartOperationBinding()
        {
            var messagePart = new MimePart { Extensions = { CreateSoapBodyBinding() } };

            foreach (var headerBinding in requiredHeaders.Select(CreateSoapHeaderBinding))
            {
                var element = CreateElement(PrefixConstants.SOAP, "header", NamespaceConstants.SOAP);

                element.SetAttribute("message", standardHeaderName);
                element.SetAttribute("part", headerBinding.Part);
                element.SetAttribute("use", headerBinding.Use == SoapBindingUse.Encoded ? "encoded" : "literal");

                if (headerBinding.Namespace != null)
                    element.SetAttribute("namespace", headerBinding.Namespace);

                if (headerBinding.Encoding != null)
                    element.SetAttribute("encodingStyle", headerBinding.Encoding);

                messagePart.Extensions.Add(element);
            }

            return messagePart;
        }

        private void BuildOperationBinding(OperationBinding operationBinding, MethodInfo methodContract)
        {
            AddSoapOperationBinding(operationBinding);

            if (protocol == XRoadProtocol.Version20 && methodContract.HasMultipartRequest())
            {
                operationBinding.Input.Extensions.Add(
                    new MimeMultipartRelatedBinding
                    {
                        Parts =
                        {
                            BuildMultipartOperationBinding(),
                            new MimePart { Extensions = { new MimeContentBinding { Part = "p1", Type = "application/binary" } } }
                        }
                    });
            }
            else
            {
                operationBinding.Input.Extensions.Add(CreateSoapBodyBinding());
                foreach (var headerBinding in requiredHeaders.Select(CreateSoapHeaderBinding))
                    operationBinding.Input.Extensions.Add(headerBinding);
            }

            if (protocol == XRoadProtocol.Version20 && methodContract.HasMultipartResponse())
            {
                operationBinding.Output.Extensions.Add(
                    new MimeMultipartRelatedBinding
                    {
                        Parts =
                        {
                            BuildMultipartOperationBinding(),
                            new MimePart { Extensions = { new MimeContentBinding { Part = "p2", Type = "application/binary" } } }
                        }
                    });
            }
            else
            {
                operationBinding.Output.Extensions.Add(CreateSoapBodyBinding());
                foreach (var headerBinding in requiredHeaders.Select(CreateSoapHeaderBinding))
                    operationBinding.Output.Extensions.Add(headerBinding);
            }
        }

        private void AddBinaryAttribute(XmlSchemaAnnotated element)
        {
            if (protocol == XRoadProtocol.Version20)
                return;

            if (schemaImports.All(import => import.Namespace != NamespaceConstants.XMIME))
                schemaImports.Add(new XmlSchemaImport { Namespace = NamespaceConstants.XMIME, SchemaLocation = NamespaceConstants.XMIME });

            element.UnhandledAttributes = new[] { CreateAttribute(PrefixConstants.XMIME, "expectedContentTypes", NamespaceConstants.XMIME, "application/octet-stream") };
        }

        private string GetBinaryNamespace()
        {
            return protocol == XRoadProtocol.Version20 ? NamespaceConstants.SOAP_ENC : NamespaceConstants.XSD;
        }

        private void AddMessageTypes(IDictionary<MethodInfo, List<string>> contractMessages)
        {
            Func<KeyValuePair<MethodInfo, List<string>>, IEnumerable<Tuple<string, MethodInfo>>> selector =
                m => protocol == XRoadProtocol.Version20
                    ? m.Value.Select(n => Tuple.Create(n, m.Key))
                    : Enumerable.Repeat(Tuple.Create(GetOperationNameFromMethodInfo(m.Key), m.Key), 1);

            foreach (var operation in contractMessages.SelectMany(selector))
                AddMessageType(operation.Item1, operation.Item2);
        }

        private Tuple<XmlQualifiedName, XmlQualifiedName> GetOperationTypeName(string operationName, MethodInfo methodContract)
        {
            var name = protocol == XRoadProtocol.Version20 ? operationName : GetOperationNameFromMethodInfo(methodContract);

            var requestTypeName = string.Format(requestTypeNameFormat, name);
            var responseTypeName = string.Format(responseTypeNameFormat, name);

            Tuple<MethodInfo, XmlSchemaComplexType, XmlSchemaComplexType> value;
            if (!operationTypes.TryGetValue(requestTypeName, out value) || methodContract != value.Item1)
                throw new Exception($"Unrecognized type `{requestTypeName}`");

            return Tuple.Create(new XmlQualifiedName(requestTypeName, targetNamespace),
                                new XmlQualifiedName(responseTypeName, targetNamespace));
        }

        private XmlSchemaGroupBase CreateOperationRequestSequence(MethodInfo method, string operationName)
        {
            var requestElement = new XmlSchemaElement { Name = "request" };

            var parameters = method.GetParameters().Where(p => (!version.HasValue || p.IsParameterInVersion(version.Value))).ToList();

            if (protocol == XRoadProtocol.Version20 || parameters.Count > 1)
            {
                var schemaParticle = operationConfiguration?.GetParameterLayout(method) == XRoadContentLayoutMode.Flexible
                    ? (XmlSchemaGroupBase)new XmlSchemaAll()
                    : new XmlSchemaSequence();

                foreach (var parameter in parameters)
                {
                    var parameterName = parameter.GetParameterName(operationConfiguration, operationName);
                    var parameterIsOptional = parameter.GetParameterIsOptional();

                    var parameterElement = new XmlSchemaElement { Name = parameterName, Annotation = CreateAnnotationElement(parameter) };

                    AddSchemaType(parameterElement, parameter.ParameterType, parameterIsOptional, parameter.GetQualifiedTypeName());

                    schemaParticle.Items.Add(parameterElement);
                }

                if (protocol == XRoadProtocol.Version20)
                    return schemaParticle;

                requestElement.SchemaType = new XmlSchemaComplexType { Particle = schemaParticle };
            }
            else if (parameters.Count == 1)
                AddSchemaType(requestElement, parameters.Single().ParameterType, false, parameters.Single().GetQualifiedTypeName());
            else requestElement.SchemaType = new XmlSchemaComplexType();

            return new XmlSchemaSequence { Items = { requestElement } };
        }

        private XmlSchemaComplexType CreateResponseType(string responseName, MethodInfo method, XmlSchemaGroupBase requestSequence, string operationName)
        {
            if (protocol == XRoadProtocol.Version20)
            {
                if (method.ReturnType == typeof(void) || !method.ReturnType.IsArray)
                    return new XmlSchemaComplexType { Name = responseName, Particle = new XmlSchemaSequence(), Annotation = CreateAnnotationElement(method) };

                var tempElement = new XmlSchemaElement();
                AddSchemaType(tempElement, method.ReturnType, false, null);

                var complexContent = (XmlSchemaComplexContent)((XmlSchemaComplexType)tempElement.SchemaType).ContentModel;

                return new XmlSchemaComplexType { Name = responseName, ContentModel = complexContent, Annotation = CreateAnnotationElement(method) };
            }

            var responseSequence = new XmlSchemaSequence();

            if (method.ReturnType != typeof(void))
            {
                var elementName = method.ReturnParameter.GetParameterName(operationConfiguration, operationName);
                if (string.IsNullOrWhiteSpace(elementName))
                    throw new Exception($"Method `{operationName}` return parameter element name cannot be empty.");

                var valueElement = new XmlSchemaElement { Name = elementName };
                AddSchemaType(valueElement, method.ReturnType, false, null);

                responseSequence.Items.Add(new XmlSchemaChoice { Items = { CreateFaultSequence(), valueElement } });
            }
            else
            {
                var faultSequence = CreateFaultSequence();
                faultSequence.MinOccurs = 0;
                responseSequence.Items.Add(faultSequence);
            }

            var responseElement = new XmlSchemaElement { Name = "response", SchemaType = new XmlSchemaComplexType { Particle = responseSequence } };

            return new XmlSchemaComplexType
            {
                Name = responseName,
                Particle = new XmlSchemaSequence { Items = { requestSequence.Items[0], responseElement } },
                Annotation = CreateAnnotationElement(method)
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

        private void AddOperationTypes(XmlSchema schema)
        {
            foreach (var operationType in operationTypes.Values.OrderBy(x => x.Item2.Name))
            {
                schema.Items.Add(operationType.Item2);
                if (protocol != XRoadProtocol.Version20 || operationType.Item1.ReturnType.IsArray)
                    schema.Items.Add(operationType.Item3);
            }
        }

        private void AddSchemaElements(XmlSchema schema)
        {
            if (protocol == XRoadProtocol.Version20)
                return;

            foreach (var schemaElement in schemaElements)
                schema.Items.Add(schemaElement.Value);
        }

        private Message CreateOperationMessage(XmlSchemaElement element, MethodInfo methodContract, Message inputMessage = null)
        {
            if (protocol != XRoadProtocol.Version20)
                return new Message
                {
                    Name = element.Name,
                    Parts = { new MessagePart { Name = "body", Element = new XmlQualifiedName(element.Name, targetNamespace) } }
                };

            var message = new Message { Name = element.SchemaTypeName.Name };

            if (inputMessage != null)
                message.Parts.Add(new MessagePart { Name = "paring", Type = inputMessage.Parts[0].Type });

            if (inputMessage != null && !methodContract.ReturnType.IsArray)
                message.Parts.Add(new MessagePart { Name = "keha", Type = GetSimpleTypeName(methodContract.ReturnType) ?? GetComplexTypeName(methodContract.ReturnType) });
            else message.Parts.Add(new MessagePart { Name = "keha", Type = element.SchemaTypeName });

            if (inputMessage == null && methodContract.HasMultipartRequest())
                message.Parts.Add(new MessagePart { Name = "p1", Type = GetSimpleTypeName(typeof(Stream)) });

            if (inputMessage != null && methodContract.HasMultipartResponse())
                message.Parts.Add(new MessagePart { Name = "p2", Type = GetSimpleTypeName(typeof(Stream)) });

            return message;
        }

        private XmlSchemaAnnotation CreateAnnotationElement(ICustomAttributeProvider provider)
        {
            var nodes = provider.GetXRoadTitles()
                                .Where(title => !string.IsNullOrWhiteSpace(title.Item2))
                                .Select(title => CreateXRoadTitleElement(title.Item1, title.Item2))
                                .ToArray();

            return nodes.Length > 0 ? new XmlSchemaAnnotation { Items = { new XmlSchemaAppInfo { Markup = nodes } } } : null;
        }

        private XmlElement CreateDocumentationElement(ICustomAttributeProvider provider)
        {
            var nodes = provider.GetXRoadTitles()
                                .Where(title => !string.IsNullOrWhiteSpace(title.Item2))
                                .Select(title => CreateXRoadTitleElement(title.Item1, title.Item2))
                                .ToArray();

            if (nodes.Length < 1)
                return null;

            var documentationElement = CreateElement("wsdl", "documentation", NamespaceConstants.WSDL);

            foreach (var node in nodes)
                documentationElement.AppendChild(node);

            return documentationElement;
        }

        private XmlNode CreateXRoadTitleElement(string languageCode, string value)
        {
            var titleElement = CreateElement(protocol.GetPrefix(), "title", xroadNamespace);
            titleElement.InnerText = value;

            if (!string.IsNullOrWhiteSpace(languageCode))
                titleElement.Attributes.Append(CreateAttribute("xml", "lang", null, languageCode));

            return titleElement;
        }

        private XmlElement CreateAddressBindingElement()
        {
            var addressElement = CreateElement(protocol.GetPrefix(), "address", xroadNamespace);
            addressElement.Attributes.Append(CreateAttribute(null, "producer", null, environmentProducerName));
            return addressElement;
        }

        private XmlElement CreateXRoadVersionBindingElement(uint value)
        {
            var addressElement = CreateElement(protocol.GetPrefix(), "version", xroadNamespace);
            addressElement.InnerText = $"v{value}";
            return addressElement;
        }

        #endregion
    }
}
