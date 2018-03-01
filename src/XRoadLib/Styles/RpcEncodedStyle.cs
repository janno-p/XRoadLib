using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Extensions;
using XRoadLib.Wsdl;

namespace XRoadLib.Styles
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

        private XmlAttribute CreateArrayTypeAttribute(XName qualifiedName)
        {
            var attribute = document.CreateAttribute(PrefixConstants.WSDL, "arrayType", NamespaceConstants.WSDL);
            attribute.Value = $"{qualifiedName.NamespaceName}:{qualifiedName.LocalName}[]";
            return attribute;
        }

        public override void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, Action<string> addSchemaImport)
        {
            addSchemaImport(NamespaceConstants.SOAP_ENC);

            var schemaAttribute = new XmlSchemaAttribute { RefName = new XmlQualifiedName("arrayType", NamespaceConstants.SOAP_ENC) };

            if (itemElement.SchemaTypeName != null)
            {
                var attribute = CreateArrayTypeAttribute(XName.Get(itemElement.SchemaTypeName.Name, itemElement.SchemaTypeName.Namespace));
                schemaAttribute.UnhandledAttributes = new[] { attribute };
            }

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

        public override bool UseElementInMessagePart => false;

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

        public override void WriteSoapEnvelope(XmlWriter writer, string producerNamespace)
        {
            base.WriteSoapEnvelope(writer, producerNamespace);

            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.SOAP_ENC, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENC);
            writer.WriteAttributeString("encodingStyle", NamespaceConstants.SOAP_ENV, NamespaceConstants.SOAP_ENC);
        }
    }
}