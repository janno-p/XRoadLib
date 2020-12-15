using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Extensions;
using XRoadLib.Wsdl;

namespace XRoadLib.Styles
{
    public class RpcEncodedStyle : Style
    {
        public override Task WriteExplicitTypeAsync(XmlWriter writer, XName qualifiedName)
        {
            return writer.WriteTypeAttributeAsync(qualifiedName);
        }

        public override async Task WriteExplicitArrayTypeAsync(XmlWriter writer, XName itemQualifiedName, int arraySize)
        {
            await writer.WriteTypeAttributeAsync("Array", NamespaceConstants.SoapEnc).ConfigureAwait(false);
            await writer.WriteArrayTypeAttributeAsync(itemQualifiedName, arraySize).ConfigureAwait(false);
        }

        private XmlAttribute CreateArrayTypeAttribute(XName qualifiedName)
        {
            var attribute = Document.CreateAttribute(PrefixConstants.Wsdl, "arrayType", NamespaceConstants.Wsdl);
            attribute.Value = $"{qualifiedName.NamespaceName}:{qualifiedName.LocalName}[]";
            return attribute;
        }

        public override void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, Action<string> addSchemaImport)
        {
            addSchemaImport(NamespaceConstants.SoapEnc);

            var schemaAttribute = new XmlSchemaAttribute { RefName = new XmlQualifiedName("arrayType", NamespaceConstants.SoapEnc) };

            if (itemElement.SchemaTypeName != null)
            {
                var attribute = CreateArrayTypeAttribute(XName.Get(itemElement.SchemaTypeName.Name, itemElement.SchemaTypeName.Namespace));
                schemaAttribute.UnhandledAttributes = new[] { attribute };
            }

            var restriction = new XmlSchemaComplexContentRestriction
            {
                BaseTypeName = new XmlQualifiedName("Array", NamespaceConstants.SoapEnc),
                Particle = new XmlSchemaSequence { Items = { itemElement } },
                Attributes = { schemaAttribute }
            };

            arrayElement.SchemaType = new XmlSchemaComplexType { ContentModel = new XmlSchemaComplexContent { Content = restriction } };
        }

        public override SoapBinding CreateSoapBinding()
        {
            return new()
            {
                Style = SoapBindingStyle.Rpc,
                Transport = NamespaceConstants.Http
            };
        }

        public override bool UseElementInMessagePart => false;

        public override SoapBodyBinding CreateSoapBodyBinding(string targetNamespace)
        {
            return new() { Use = SoapBindingUse.Encoded, Namespace = targetNamespace, Encoding = NamespaceConstants.SoapEnc };
        }

        public override SoapHeaderBinding CreateSoapHeaderBinding(XName headerName, string messageName, string targetNamespace)
        {
            return new()
            {
                Message = new XmlQualifiedName(messageName, targetNamespace),
                Part = headerName.LocalName,
                Use = SoapBindingUse.Encoded,
                Namespace = headerName.NamespaceName,
                Encoding = NamespaceConstants.SoapEnc
            };
        }

        public override SoapOperationBinding CreateSoapOperationBinding(string soapAction)
        {
            return new() { SoapAction = soapAction, Style = SoapBindingStyle.Rpc };
        }
    }
}