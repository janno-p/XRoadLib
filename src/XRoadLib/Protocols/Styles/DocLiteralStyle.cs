using System.Collections.Generic;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Schema;

namespace XRoadLib.Protocols.Styles
{
    public class DocLiteralStyle : Style
    {
        public override XmlElement CreateSoapHeader(SoapHeaderBinding binding)
        {
            var element = document.CreateElement(PrefixConstants.SOAP, "header", NamespaceConstants.SOAP);

            element.SetAttribute("message", binding.Message.Name);
            element.SetAttribute("part", binding.Part);
            element.SetAttribute("use", "literal");

            return element;
        }

        public override void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, ISet<string> requiredImports)
        {
            arrayElement.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { itemElement } } };
        }

        public override SoapBinding CreateSoapBinding()
        {
            return new SoapBinding
            {
                Style = SoapBindingStyle.Document,
                Transport = NamespaceConstants.HTTP
            };
        }

        public override void AddInputMessageParts(IProtocol protocol, OperationDefinition operationDefinition, Message message)
        {
            var xname = operationDefinition.Name;
            var qualifiedName = new XmlQualifiedName(xname.LocalName, xname.NamespaceName);
            message.Parts.Add(new MessagePart { Name = "body", Element = qualifiedName });
        }

        public override void AddOutputMessageParts(IProtocol protocol, OperationDefinition operationDefinition, Message message)
        {
            var xname = operationDefinition.Name;
            var qualifiedName = new XmlQualifiedName($"{xname.LocalName}Response", xname.NamespaceName);
            message.Parts.Add(new MessagePart { Name = "body", Element = qualifiedName });
        }

        public override SoapBodyBinding CreateSoapBodyBinding(string targetNamespace)
        {
            return new SoapBodyBinding { Use = SoapBindingUse.Literal };
        }

        public override SoapHeaderBinding CreateSoapHeaderBinding(XName headerName, string messageName, string targetNamespace)
        {
            return new SoapHeaderBinding
            {
                Message = new XmlQualifiedName(messageName, targetNamespace),
                Part = headerName.LocalName,
                Use = SoapBindingUse.Literal
            };
        }

        public override SoapOperationBinding CreateSoapOperationBinding()
        {
            return new SoapOperationBinding { SoapAction = "", Style = SoapBindingStyle.Document };
        }
    }
}