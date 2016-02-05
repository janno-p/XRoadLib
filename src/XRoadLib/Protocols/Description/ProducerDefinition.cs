using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Protocols.Description
{
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

        private readonly IDictionary<XName, TypeDefinition> schemaTypeDefinitions = new Dictionary<XName, TypeDefinition>();
        private readonly IDictionary<Type, TypeDefinition> runtimeTypeDefinitions = new Dictionary<Type, TypeDefinition>();
        private readonly ISet<string> requiredImports = new SortedSet<string>();

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

        public void SaveTo(Stream stream)
        {
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n" }))
            {
                writer.WriteStartDocument();

                WriteServiceDescription(writer);
                writer.Flush();

                writer.WriteEndDocument();
                writer.Flush();
            }
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
                                                  .Where(type => type.IsXRoadSerializable())
                                                  .Where(type => !version.HasValue || type.ExistsInVersion(version.Value))
                                                  .Select(type => schemaDefinitionReader.GetTypeDefinition(type))
                                                  .Where(def => !def.IsAnonymous && def.Name != null && def.State == DefinitionState.Default);

            foreach (var typeDefinition in typeDefinitions)
            {
                if (schemaTypeDefinitions.ContainsKey(typeDefinition.Name))
                    throw new Exception($"Multiple type definitions for same name `{typeDefinition.Name}`.");

                schemaTypeDefinitions.Add(typeDefinition.Name, typeDefinition);
                runtimeTypeDefinitions.Add(typeDefinition.Type, typeDefinition);
            }
        }

        private XmlSchema BuildSchema(string targetNamespace, MessageCollection messages)
        {
            var schema = new XmlSchema { TargetNamespace = targetNamespace };

            var typeDefinitions = schemaTypeDefinitions.Where(x => x.Key.NamespaceName == targetNamespace)
                                                       .OrderBy(x => x.Key.LocalName)
                                                       .Select(x => x.Value);

            foreach (var typeDefinition in typeDefinitions)
            {
                var schemaType = new XmlSchemaComplexType
                {
                    Name = typeDefinition.Name.LocalName,
                    IsAbstract = typeDefinition.Type.IsAbstract,
                    Annotation = CreateSchemaAnnotation(typeDefinition)
                };

                AddComplexTypeContent(schemaType, typeDefinition, targetNamespace);

                schema.Items.Add(schemaType);
            }

            var operationDefinitions = contractAssembly.GetServiceContracts()
                                                       .SelectMany(x => x.Value
                                                                         .Where(op => !version.HasValue || op.ExistsInVersion(version.Value))
                                                                         .Select(op => schemaDefinitionReader.GetOperationDefinition(x.Key, XName.Get(op.Name, protocol.ProducerNamespace), version)))
                                                       .Where(def => def.State == DefinitionState.Default)
                                                       .OrderBy(def => def.Name.LocalName)
                                                       .ToList();

            foreach (var operationDefinition in operationDefinitions)
            {
                var methodParameters = operationDefinition.MethodInfo.GetParameters();
                if (methodParameters.Length > 1)
                    throw new Exception($"Invalid X-Road operation contract `{operationDefinition.Name.LocalName}`: expected 0-1 input parameters, but {methodParameters.Length} was given.");

                var parameterInfo = methodParameters.SingleOrDefault();
                var requestElement = parameterInfo != null
                    ? CreateContentElement(new RequestValueDefinition(parameterInfo, operationDefinition), targetNamespace)
                    : new XmlSchemaElement { Name = "request", SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence() } };

                if (protocol.Style.UseElementInMessagePart)
                    schema.Items.Add(new XmlSchemaElement
                    {
                        Name = operationDefinition.Name.LocalName,
                        SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElement } } }
                    });

                var responseValueDefinition = schemaDefinitionReader.GetResponseValueDefinition(operationDefinition);

                XmlSchemaElement responseElement;
                XmlSchemaElement resultElement = null;

                var outputType = operationDefinition.MethodInfo.ReturnType;

                if (responseValueDefinition.HasExplicitXRoadFault)
                {
                    var outputParticle = new XmlSchemaSequence();
                    responseElement = new XmlSchemaElement { Name = "response", SchemaType = new XmlSchemaComplexType { Particle = outputParticle } };

                    var faultSequence = new XmlSchemaSequence
                    {
                        Items =
                        {
                            new XmlSchemaElement { Name = "faultCode", SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) },
                            new XmlSchemaElement { Name = "faultString", SchemaTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) }
                        }
                    };

                    if (outputType == typeof(void))
                    {
                        faultSequence.MinOccurs = 0;
                        outputParticle.Items.Add(faultSequence);
                    }
                    else
                    {
                        resultElement = CreateContentElement(responseValueDefinition, targetNamespace);
                        outputParticle.Items.Add(new XmlSchemaChoice { Items = { faultSequence, resultElement } });
                    }
                }
                else responseElement = resultElement = CreateContentElement(responseValueDefinition, targetNamespace);

                if (protocol.Style.UseElementInMessagePart)
                    schema.Items.Add(new XmlSchemaElement
                    {
                        Name = $"{operationDefinition.Name.LocalName}Response",
                        SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { requestElement, responseElement } } }
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
                    var responseTypeName = resultElement?.SchemaTypeName;
                    outputMessage.Parts.Add(new MessagePart { Name = protocol.RequestPartNameInResponse, Type = requestTypeName });
                    outputMessage.Parts.Add(new MessagePart { Name = protocol.ResponsePartNameInResponse, Type = responseTypeName });
                }

                if (operationDefinition.OutputBinaryMode == BinaryMode.Attachment)
                    outputMessage.Parts.Add(new MessagePart { Name = "file", Type = new XmlQualifiedName("base64Binary", NamespaceConstants.XSD) });

                messages.Add(outputMessage);

                var operation = new Operation
                {
                    Name = operationDefinition.Name.LocalName,
                    Messages =
                    {
                        new OperationInput { Message = new XmlQualifiedName(inputMessage.Name, targetNamespace) },
                        new OperationOutput { Message = new XmlQualifiedName(outputMessage.Name, targetNamespace) }
                    }
                };

                portType.Operations.Add(operation);

                var inputBinding = new InputBinding();
                if (operationDefinition.InputBinaryMode == BinaryMode.Attachment)
                {
                    var soapPart = new MimePart();
                    AddOperationContentBinding(soapPart.Extensions, protocol.Style.CreateSoapHeader);
                    var filePart = new MimePart { Extensions = { new MimeContentBinding { Part = "file", Type = "application/binary" } } };
                    inputBinding.Extensions.Add(new MimeMultipartRelatedBinding { Parts = { soapPart, filePart } });
                }
                else AddOperationContentBinding(inputBinding.Extensions, x => x);

                var outputBinding = new OutputBinding();
                if (operationDefinition.OutputBinaryMode == BinaryMode.Attachment)
                {
                    var soapPart = new MimePart();
                    AddOperationContentBinding(soapPart.Extensions, protocol.Style.CreateSoapHeader);
                    var filePart = new MimePart { Extensions = { new MimeContentBinding { Part = "file", Type = "application/binary" } } };
                    outputBinding.Extensions.Add(new MimeMultipartRelatedBinding { Parts = { soapPart, filePart } });
                }
                else AddOperationContentBinding(outputBinding.Extensions, x => x);

                binding.Operations.Add(new OperationBinding
                {
                    Name = operationDefinition.Name.LocalName,
                    Extensions = { protocol.CreateOperationVersionElement(operationDefinition), protocol.Style.CreateSoapOperationBinding() },
                    Input = inputBinding,
                    Output = outputBinding
                });
            }

            foreach (var requiredImport in requiredImports)
                schema.Includes.Add(new XmlSchemaImport { Namespace = requiredImport, SchemaLocation = requiredImport });

            return schema;
        }

        private void AddOperationContentBinding<THeader>(ServiceDescriptionFormatExtensionCollection extensions, Func<SoapHeaderBinding, THeader> projectionFunc)
        {
            extensions.Add(protocol.Style.CreateSoapBodyBinding(protocol.ProducerNamespace));
            foreach (var headerBinding in protocol.MandatoryHeaders.Select(name => protocol.Style.CreateSoapHeaderBinding(name, STANDARD_HEADER_NAME, protocol.ProducerNamespace)).Select(projectionFunc))
                extensions.Add(headerBinding);
        }

        private void AddComplexTypeContent(XmlSchemaComplexType schemaType, TypeDefinition typeDefinition, string targetNamespace)
        {
            var contentParticle = new XmlSchemaSequence();

            foreach (var propertyDefinition in GetDescriptionProperties(typeDefinition))
                contentParticle.Items.Add(CreateContentElement(propertyDefinition, targetNamespace));

            if (typeDefinition.Type.BaseType != typeof(XRoadSerializable))
            {
                var extension = new XmlSchemaComplexContentExtension
                {
                    BaseTypeName = GetSchemaTypeName(typeDefinition.Type.BaseType, targetNamespace),
                    Particle = contentParticle
                };

                schemaType.ContentModel = new XmlSchemaComplexContent { Content = extension };
            }
            else schemaType.Particle = contentParticle;
        }

        private XmlQualifiedName GetSchemaTypeName(Type type, string targetNamespace)
        {
            var name = type.GetSystemTypeName();
            if (name != null)
                return new XmlQualifiedName(name.LocalName, name.NamespaceName);

            TypeDefinition typeDefinition;
            if (!runtimeTypeDefinitions.TryGetValue(type, out typeDefinition))
                throw new Exception($"Unrecognized type `{type.FullName}`.");

            AddRequiredImport(targetNamespace, typeDefinition.Name.NamespaceName);

            return new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
        }

        private XmlSchemaAnnotation CreateSchemaAnnotation(Definition definition)
        {
            var nodes = definition.Documentation
                                  .Select(doc => protocol.CreateTitleElement(doc.Item1, doc.Item2))
                                  .Cast<XmlNode>()
                                  .ToArray();

            return definition.Documentation.Any() ? new XmlSchemaAnnotation { Items = { new XmlSchemaAppInfo { Markup = nodes } } } : null;
        }

        private void AddBinaryAttribute(XmlSchemaAnnotated schemaElement)
        {
            requiredImports.Add(NamespaceConstants.XMIME);

            schemaElement.UnhandledAttributes = new[] { protocol.Style.CreateExpectedContentType("application/octet-stream") };
        }

        private TypeDefinition GetContentTypeDefinition(IContentDefinition contentDefinition)
        {
            if (contentDefinition.TypeName != null)
                return schemaTypeDefinitions[contentDefinition.TypeName];

            if (runtimeTypeDefinitions.ContainsKey(contentDefinition.RuntimeType))
                return runtimeTypeDefinitions[contentDefinition.RuntimeType];

            return schemaDefinitionReader.GetTypeDefinition(contentDefinition.RuntimeType);
        }

        private void AddRequiredImport(string targetNamespace, string typeNamespace)
        {
            if (typeNamespace != NamespaceConstants.XSD && typeNamespace != targetNamespace)
                requiredImports.Add(typeNamespace);
        }

        private void SetSchemaElementType(XmlSchemaElement schemaElement, IContentDefinition contentDefinition, string targetNamespace)
        {
            if (typeof(Stream).IsAssignableFrom(contentDefinition.RuntimeType) && contentDefinition.UseXop)
                AddBinaryAttribute(schemaElement);

            var typeDefinition = GetContentTypeDefinition(contentDefinition);
            if (!typeDefinition.IsAnonymous)
            {
                AddRequiredImport(targetNamespace, typeDefinition.Name.NamespaceName);
                schemaElement.SchemaTypeName = new XmlQualifiedName(typeDefinition.Name.LocalName, typeDefinition.Name.NamespaceName);
                return;
            }

            XmlSchemaType schemaType;
            if (contentDefinition.RuntimeType.IsEnum)
            {
                schemaType = new XmlSchemaSimpleType();
                AddEnumTypeContent(contentDefinition.RuntimeType, (XmlSchemaSimpleType)schemaType);
            }
            else
            {
                schemaType = new XmlSchemaComplexType();
                AddComplexTypeContent((XmlSchemaComplexType)schemaType, typeDefinition, targetNamespace);
            }
            schemaType.Annotation = CreateSchemaAnnotation(typeDefinition);

            schemaElement.SchemaType = schemaType;
        }

        private static void AddEnumTypeContent(Type type, XmlSchemaSimpleType schemaType)
        {
            var restriction = new XmlSchemaSimpleTypeRestriction { BaseTypeName = new XmlQualifiedName("string", NamespaceConstants.XSD) };

            foreach (var name in Enum.GetNames(type))
            {
                var memberInfo = type.GetMember(name).Single();
                var attribute = memberInfo.GetSingleAttribute<XmlEnumAttribute>();
                restriction.Facets.Add(new XmlSchemaEnumerationFacet { Value = (attribute?.Name).GetValueOrDefault(name) });
            }

            schemaType.Content = restriction;
        }

        private XmlSchemaElement CreateContentElement(ContentDefinition propertyDefinition, string targetNamespace)
        {
            var schemaElement = new XmlSchemaElement
            {
                Name = propertyDefinition.Name?.LocalName,
                Annotation = CreateSchemaAnnotation(propertyDefinition)
            };

            if (propertyDefinition.Name == null)
            {
                schemaElement.Name = propertyDefinition.ArrayItemDefinition.Name.LocalName;

                if (propertyDefinition.ArrayItemDefinition.IsOptional)
                    schemaElement.MinOccurs = 0;

                schemaElement.IsNillable = propertyDefinition.ArrayItemDefinition.IsNullable;

                schemaElement.MaxOccursString = "unbounded";

                SetSchemaElementType(schemaElement, propertyDefinition.ArrayItemDefinition, targetNamespace);

                return schemaElement;
            }

            if (propertyDefinition.IsOptional)
                schemaElement.MinOccurs = 0;

            schemaElement.IsNillable = propertyDefinition.IsNullable;

            if (propertyDefinition.ArrayItemDefinition == null)
            {
                SetSchemaElementType(schemaElement, propertyDefinition, targetNamespace);
                return schemaElement;
            }

            var itemElement = new XmlSchemaElement
            {
                Name = propertyDefinition.ArrayItemDefinition.Name.LocalName,
                MaxOccursString = "unbounded"
            };

            if (propertyDefinition.ArrayItemDefinition.IsOptional)
                itemElement.MinOccurs = 0;

            itemElement.IsNillable = propertyDefinition.ArrayItemDefinition.IsNullable;

            SetSchemaElementType(itemElement, propertyDefinition.ArrayItemDefinition, targetNamespace);

            protocol.Style.AddItemElementToArrayElement(schemaElement, itemElement, requiredImports);

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

            serviceDescription.Types.Schemas.Add(BuildSchema(protocol.ProducerNamespace, serviceDescription.Messages));
            serviceDescription.PortTypes.Add(portType);

            binding.Extensions.Add(protocol.Style.CreateSoapBinding());
            serviceDescription.Bindings.Add(binding);

            servicePort.Extensions.Add(new SoapAddressBinding { Location = "" });

            serviceDescription.Services.Add(service);

            protocol.ExportServiceDescription(serviceDescription);

            serviceDescription.Write(writer);
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
            return typeDefinition.Type
                                 .GetPropertiesSorted(typeDefinition.ContentComparer, version, p => schemaDefinitionReader.GetPropertyDefinition(p, typeDefinition))
                                 .Where(d => d.State == DefinitionState.Default);
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
