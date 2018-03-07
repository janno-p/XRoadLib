using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Wsdl;

namespace XRoadLib.Styles
{
    public class DocLiteralStyle : Style
    {
        public override void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, Action<string> addSchemaImport)
        {
            arrayElement.SchemaType = new XmlSchemaComplexType { Particle = new XmlSchemaSequence { Items = { itemElement } } };
        }

        public override SoapBinding CreateSoapBinding()
        {
            return new SoapBinding
            {
                Transport = NamespaceConstants.HTTP
            };
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