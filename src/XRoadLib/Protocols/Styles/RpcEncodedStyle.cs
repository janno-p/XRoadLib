using System.Collections.Generic;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Extensions;
using XRoadLib.Schema;

namespace XRoadLib.Protocols.Styles
{
    public class RpcEncodedStyle : Style
    {
        public override void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        {
            writer.WriteTypeAttribute(qualifiedName);
        }

        public override void WriteExplicitArrayType(XmlWriter writer, XName itemQualifiedName, int arraySize)
        {
            writer.WriteTypeAttribute("Array", NamespaceConstants.SOAP_ENC);
            writer.WriteArrayTypeAttribute(itemQualifiedName, arraySize);
        }

        public override XmlElement CreateSoapHeader(SoapHeaderBinding binding)
        {
            var element = document.CreateElement(PrefixConstants.SOAP, "header", NamespaceConstants.SOAP);

            element.SetAttribute("message", binding.Message.Name);
            element.SetAttribute("part", binding.Part);
            element.SetAttribute("use", "encoded");
            element.SetAttribute("namespace", binding.Namespace);
            element.SetAttribute("encodingStyle", binding.Encoding);

            return element;
        }

        public override XmlAttribute CreateArrayTypeAttribute(XName qualifiedName)
        {
            var attribute = document.CreateAttribute(PrefixConstants.WSDL, "arrayType", NamespaceConstants.WSDL);
            attribute.Value = $"{qualifiedName.NamespaceName}:{qualifiedName.LocalName}[]";
            return attribute;
        }

        public override void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, ISet<string> requiredImports)
        {
            requiredImports.Add(NamespaceConstants.SOAP_ENC);

            var schemaAttribute = new XmlSchemaAttribute { RefName = new XmlQualifiedName("arrayType", NamespaceConstants.SOAP_ENC) };

            if (itemElement.SchemaTypeName != null)
                schemaAttribute.UnhandledAttributes = new[] { CreateArrayTypeAttribute(XName.Get(itemElement.SchemaTypeName.Name, itemElement.SchemaTypeName.Namespace)) };

            var restriction = new XmlSchemaComplexContentRestriction
            {
                BaseTypeName = new XmlQualifiedName("Array", NamespaceConstants.SOAP_ENC),
                Particle = new XmlSchemaSequence { Items = { itemElement } },
                Attributes = { schemaAttribute }
            };

            arrayElement.SchemaType = new XmlSchemaComplexType { ContentModel = new XmlSchemaComplexContent { Content = restriction } };
        }

        public override SoapBinding CreateSoapBinding()
        {
            return new SoapBinding
            {
                Style = SoapBindingStyle.Rpc,
                Transport = NamespaceConstants.HTTP
            };
        }

        public override void AddInputMessageParts(IProtocol protocol, OperationDefinition operationDefinition, Message message)
        {
            var xname = operationDefinition.OperationTypeDefinition.InputName;
            var qualifiedName = new XmlQualifiedName(xname.LocalName, xname.NamespaceName);
            message.Parts.Add(new MessagePart { Name = protocol.RequestPartNameInRequest, Type = qualifiedName });
        }

        public override void AddOutputMessageParts(IProtocol protocol, OperationDefinition operationDefinition, Message message)
        {
            var inputName = operationDefinition.OperationTypeDefinition.InputName;
            var inputQualifiedName = new XmlQualifiedName(inputName.LocalName, inputName.NamespaceName);
            message.Parts.Add(new MessagePart { Name = protocol.RequestPartNameInResponse, Type = inputQualifiedName });

            var outputName = operationDefinition.OperationTypeDefinition.OutputName;
            var outputQualifiedName = new XmlQualifiedName(outputName.LocalName, outputName.NamespaceName);
            message.Parts.Add(new MessagePart { Name = protocol.ResponsePartNameInResponse, Type = outputQualifiedName });
        }

        public override SoapBodyBinding CreateSoapBodyBinding(string targetNamespace)
        {
            return new SoapBodyBinding { Use = SoapBindingUse.Encoded, Namespace = targetNamespace, Encoding = NamespaceConstants.SOAP_ENC };
        }

        public override SoapHeaderBinding CreateSoapHeaderBinding(XName headerName, string messageName, string targetNamespace)
        {
            return new SoapHeaderBinding
            {
                Message = new XmlQualifiedName(messageName, targetNamespace),
                Part = headerName.LocalName,
                Use = SoapBindingUse.Encoded,
                Namespace = headerName.NamespaceName,
                Encoding = NamespaceConstants.SOAP_ENC
            };
        }

        public override SoapOperationBinding CreateSoapOperationBinding()
        {
            return new SoapOperationBinding { SoapAction = "", Style = SoapBindingStyle.Rpc };
        }
    }
}