using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using XRoadLib.Attributes;
using XRoadLib.Extensions;

namespace XRoadLib.Protocols.Description
{
    public sealed class ProducerDefinition
    {
        private const string STANDARD_HEADER_NAME = "stdhdr";

        private readonly SchemaBuilder schemaBuilder;
        private readonly XRoadSchemaBuilder xRoadSchema;

        private readonly Assembly contractAssembly;
        private readonly XRoadProtocol protocol;
        private readonly string xroadNamespace;
        private readonly string targetNamespace;
        private readonly string standardHeaderName;
        private readonly uint? version;
        private readonly IOperationConfiguration operationConfiguration;

        private readonly string requestTypeNameFormat;
        private readonly string responseTypeNameFormat;
        private readonly string requestMessageNameFormat;
        private readonly string responseMessageNameFormat;

        private readonly Binding binding;
        private readonly PortType portType;
        private readonly Port servicePort;
        private readonly Service service;

        private readonly IDictionary<MethodInfo, IDictionary<string, XRoadServiceAttribute>> serviceContracts;

        private readonly ICollection<string> requiredHeaders = new SortedSet<string>();
        private readonly IList<Message> messages = new List<Message>();
        private readonly IList<XmlSchemaImport> schemaImports = new List<XmlSchemaImport>();
        private readonly IDictionary<string, XmlSchemaElement> schemaElements = new SortedDictionary<string, XmlSchemaElement>();
        private readonly IDictionary<string, Tuple<MethodInfo, XmlSchemaComplexType, XmlSchemaComplexType, XmlQualifiedName>> operationTypes = new SortedDictionary<string, Tuple<MethodInfo, XmlSchemaComplexType, XmlSchemaComplexType, XmlQualifiedName>>();

        private string location;

        public string HeaderMessage { private get; set; }

        public string Location
        {
            private get
            {
                if (!string.IsNullOrWhiteSpace(location))
                    return location;
                return protocol < XRoadProtocol.Version40 ? "http://TURVASERVER/cgi-bin/consumer_proxy" : "http://INSERT_CORRECT_SERVICE_URL";
            }
            set { location = value; }
        }

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

            if (version.HasValue && !TypeExtensions.IsVersionInRange(version.Value, producerConfiguration.MinOperationVersion, producerConfiguration.MaxOperationVersion + 1u))
                throw new ArgumentOutOfRangeException($"Web service contract does not offser support for `v{version.Value}` services in protocol version `{protocol}`.", nameof(version));
            this.version = version;

            xroadNamespace = protocol.GetNamespace();
            targetNamespace = protocol.GetProducerNamespace(producerName);
            xRoadSchema = new XRoadSchemaBuilder(protocol);

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
                Binding = new XmlQualifiedName(binding.Name, targetNamespace)
            };

            if (protocol < XRoadProtocol.Version40)
                servicePort.Extensions.Add(xRoadSchema.CreateAddressBinding(environmentProducerName.GetValueOrDefault(producerName)));

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

            serviceContracts = contractAssembly.GetServiceContracts();

            schemaBuilder = new SchemaBuilder(protocol, targetNamespace, producerConfiguration, version);
            schemaBuilder.BuildTypes(contractAssembly);

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

        private void AddOperation(string operationName, XRoadServiceAttribute operationContract, MethodInfo methodInfo)
        {
            if (operationContract.IsHidden || (version.HasValue && !operationContract.ExistsInVersion(version.Value)))
                return;

            BuildOperationElements(operationName, methodInfo, operationContract.IsAbstract);

            if (operationContract.IsAbstract)
                return;

            var serviceVersion = version.GetValueOrDefault(operationContract.AddedInVersion);

            var operationBinding = new OperationBinding
            {
                Name = operationName,
                Extensions = { xRoadSchema.CreateXRoadVersionBinding(serviceVersion) },
                Input = new InputBinding(),
                Output = new OutputBinding()
            };

            BuildOperationBinding(operationBinding, methodInfo);

            binding.Operations.Add(operationBinding);
        }

        private void WriteServiceDescription(XmlWriter writer)
        {
            AddOperations();

            var serviceDescription = new ServiceDescription { TargetNamespace = targetNamespace };
            AddServiceDescriptionNamespaces(serviceDescription);

            var schema = new XmlSchema { TargetNamespace = targetNamespace };
            CreateXmlSchemaImports(schema);

            foreach (var schemaType in schemaBuilder.SchemaTypes)
                schema.Items.Add(schemaType);

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

            if (protocol < XRoadProtocol.Version40)
                foreach (var title in contractAssembly.GetXRoadTitles().OrderBy(t => t.Item1.ToLower()))
                    servicePort.Extensions.Add(xRoadSchema.CreateXRoadTitle(title.Item1, title.Item2));

            servicePort.Extensions.Add(new SoapAddressBinding { Location = Location });

            serviceDescription.Services.Add(service);

            serviceDescription.Write(writer);
        }

        private void AddServiceContracts()
        {
            AddMessageTypes(
                serviceContracts.Select(kvp => Tuple.Create(kvp.Key,
                    kvp.Value
                        .Where(v => !v.Value.IsHidden && (!version.HasValue || v.Value.ExistsInVersion(version.Value)))
                        .ToDictionary(y => y.Key, y => y.Value)))
                    .Where(x => x.Item2.Any())
                    .ToDictionary(x => x.Item1, x => x.Item2.Keys.ToList()));
        }

        private void AddOperations()
        {
            foreach (var op in serviceContracts.SelectMany(x => x.Value.Select(y => Tuple.Create(y.Key, y.Value, x.Key))).OrderBy(x => x.Item1))
                AddOperation(op.Item1, op.Item2, op.Item3);
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
                var operation = new Operation { Name = name, DocumentationElement = xRoadSchema.CreateDocumentationFor(methodContract) };

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

            var requestType = CreateOperationRequestType(requestName, method);
            var responseType = CreateResponseType(responseName, method, (XmlSchemaGroupBase)requestType.Item2?.Particle, operationName);

            operationTypes.Add(requestName, Tuple.Create(method, requestType.Item2, responseType, requestType.Item1));
        }

        private bool IsExistingType(string typeName)
        {
            return schemaBuilder[typeName] || operationTypes.ContainsKey(typeName);
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

            var operation = new Operation { Name = name, DocumentationElement = xRoadSchema.CreateDocumentationFor(methodContract) };

            operation.Messages.Add(new OperationInput { Message = new XmlQualifiedName(inputMessage.Name, targetNamespace) });
            operation.Messages.Add(new OperationOutput { Message = new XmlQualifiedName(outputMessage.Name, targetNamespace) });

            portType.Operations.Add(operation);

            messages.Add(inputMessage);
            messages.Add(outputMessage);
        }

        private void CreateXmlSchemaImports(XmlSchema schema)
        {
            foreach (var requiredImport in schemaBuilder.RequiredImports)
                schema.Includes.Add(new XmlSchemaImport { Namespace = requiredImport, SchemaLocation = requiredImport });

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
                Use = protocol == XRoadProtocol.Version20 ? SoapBindingUse.Encoded : SoapBindingUse.Literal,
                Namespace = protocol == XRoadProtocol.Version20 ? xroadNamespace : null,
                Encoding = protocol == XRoadProtocol.Version20 ? NamespaceConstants.SOAP_ENC : null
            };
        }

        private MimePart BuildMultipartOperationBinding()
        {
            var messagePart = new MimePart { Extensions = { CreateSoapBodyBinding() } };

            foreach (var headerBinding in requiredHeaders.Select(CreateSoapHeaderBinding))
                messagePart.Extensions.Add(xRoadSchema.CreateSoapHeader(headerBinding));

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

            Tuple<MethodInfo, XmlSchemaComplexType, XmlSchemaComplexType, XmlQualifiedName> value;
            if (!operationTypes.TryGetValue(requestTypeName, out value) || methodContract != value.Item1)
                throw new Exception($"Unrecognized type `{requestTypeName}`");

            return Tuple.Create(new XmlQualifiedName(requestTypeName, targetNamespace),
                                new XmlQualifiedName(responseTypeName, targetNamespace));
        }

        private Tuple<XmlQualifiedName, XmlSchemaComplexType> CreateOperationRequestType(string requestName, MethodInfo method)
        {
            var parameters = method.GetParameters().Where(p => !version.HasValue || p.ExistsInVersion(version.Value)).ToList();
            var complexType = new XmlSchemaComplexType { Name = requestName, Annotation = schemaBuilder.CreateSchemaAnnotation(method) };
            var requestElement = new XmlSchemaElement { Name = "request" };

            if (parameters.Count == 0)
            {
                if (protocol != XRoadProtocol.Version20)
                    requestElement.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence() };

                complexType.Particle = protocol != XRoadProtocol.Version20 ? new XmlSchemaSequence { Items = { requestElement } } : new XmlSchemaSequence();

                return Tuple.Create((XmlQualifiedName)null, complexType);
            }

            if (parameters.Count == 1)
            {
                var parameter = parameters.Single();
                var parameterElement = schemaBuilder.CreateSchemaElement(parameter, true);
                if (string.IsNullOrWhiteSpace(parameterElement.Name))
                {
                    if (protocol == XRoadProtocol.Version20)
                        return parameterElement.SchemaType != null
                            ? Tuple.Create((XmlQualifiedName)null, (XmlSchemaComplexType)parameterElement.SchemaType)
                            : Tuple.Create(parameterElement.SchemaTypeName, (XmlSchemaComplexType)null);

                    parameterElement.Name = "request";
                    complexType.Particle = new XmlSchemaSequence { Items = { parameterElement } };

                    return Tuple.Create((XmlQualifiedName)null, complexType);
                }
            }

            var schemaParticle = operationConfiguration?.GetParameterLayout(method) == XRoadContentLayoutMode.Flexible
                ? (XmlSchemaGroupBase)new XmlSchemaAll()
                : new XmlSchemaSequence();

            foreach (var parameter in parameters)
                schemaParticle.Items.Add(schemaBuilder.CreateSchemaElement(parameter));

            if (protocol == XRoadProtocol.Version20)
                complexType.Particle = schemaParticle;
            else
            {
                requestElement.SchemaType = new XmlSchemaComplexType { Particle = schemaParticle };
                complexType.Particle = new XmlSchemaSequence { Items = { requestElement } };
            }

            return Tuple.Create((XmlQualifiedName)null, complexType);
        }

        private XmlSchemaComplexType CreateResponseType(string responseName, MethodInfo method, XmlSchemaGroupBase requestSequence, string operationName)
        {
            var complexType = new XmlSchemaComplexType { Name = responseName, Annotation = schemaBuilder.CreateSchemaAnnotation(method) };

            var faultSequence = schemaBuilder.CreateFaultSequence(method);
            if (faultSequence != null && method.ReturnType == typeof(void))
                faultSequence.MinOccurs = 0;

            if (protocol == XRoadProtocol.Version20)
            {
                if (method.ReturnType == typeof(void))
                {
                    complexType.Particle = faultSequence != null ? new XmlSchemaSequence { Items = { faultSequence } } : new XmlSchemaSequence();
                    return complexType;
                }

                var resultElement = schemaBuilder.CreateSchemaElement(method.ReturnParameter);
                if (string.IsNullOrWhiteSpace(resultElement.Name))
                {
                    if (faultSequence != null)
                        throw new Exception($"Method `{operationName}` return parameter element name cannot be empty.");

                    if (!method.ReturnType.IsArray)
                        return null;

                    complexType.ContentModel = (XmlSchemaComplexContent)((XmlSchemaComplexType)resultElement.SchemaType).ContentModel;
                }
                else
                {
                    complexType.Particle = faultSequence != null ? new XmlSchemaSequence { Items = { new XmlSchemaChoice { Items = { faultSequence, resultElement } } } }
                                                                 : new XmlSchemaSequence { Items = { resultElement } };
                }

                return complexType;
            }

            var responseSequence = new XmlSchemaSequence();
            var responseElement = new XmlSchemaElement { Name = "response", SchemaType = new XmlSchemaComplexType { Particle = responseSequence } };

            if (method.ReturnType != typeof(void))
            {
                var resultElement = schemaBuilder.CreateSchemaElement(method.ReturnParameter);
                if (string.IsNullOrWhiteSpace(resultElement.Name))
                {
                    if (faultSequence != null)
                        throw new Exception($"Method `{operationName}` return parameter element name cannot be empty.");

                    resultElement.Name = "response";
                    responseElement = resultElement;
                }
                else
                {
                    if (faultSequence != null)
                        responseSequence.Items.Add(new XmlSchemaChoice { Items = { faultSequence, resultElement } });
                    else responseSequence.Items.Add(resultElement);
                }
            }
            else if (faultSequence != null)
                responseSequence.Items.Add(faultSequence);

            return new XmlSchemaComplexType
            {
                Name = responseName,
                Particle = new XmlSchemaSequence { Items = { requestSequence.Items[0], responseElement } },
                Annotation = schemaBuilder.CreateSchemaAnnotation(method)
            };
        }

        private void AddOperationTypes(XmlSchema schema)
        {
            foreach (var operationType in operationTypes.Values.Select(x => x.Item2).Concat(operationTypes.Values.Select(x => x.Item3)).Where(x => x != null).OrderBy(x => x.Name))
                schema.Items.Add(operationType);
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
                message.Parts.Add(new MessagePart { Name = "keha", Type = schemaBuilder.GetSchemaTypeName(methodContract.ReturnType) });
            else message.Parts.Add(new MessagePart { Name = "keha", Type = element.SchemaTypeName });

            if (inputMessage == null && methodContract.HasMultipartRequest())
                message.Parts.Add(new MessagePart { Name = "file", Type = schemaBuilder.GetSchemaTypeName(typeof(Stream)) });

            if (inputMessage != null && methodContract.HasMultipartResponse())
                message.Parts.Add(new MessagePart { Name = "file", Type = schemaBuilder.GetSchemaTypeName(typeof(Stream)) });

            return message;
        }

        #endregion
    }
}
