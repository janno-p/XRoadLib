using System.Collections.Generic;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

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
    }
}