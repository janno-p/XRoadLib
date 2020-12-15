using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace XRoadLib.Serialization.Template
{
    public class XRoadXmlSchemaTemplate : IXmlTemplate
    {
        private readonly XmlSchema _schema;
        private readonly XmlSchemaElement _requestElement;
        private readonly XmlSchemaElement _responseElement;

        public IDictionary<string, Type> ParameterTypes => throw new NotImplementedException();
        public IEnumerable<IXmlTemplateNode> ParameterNodes => throw new NotImplementedException();
        public IXmlTemplateNode RequestNode => new XRoadXmlSchemaTemplateNode(_requestElement, _schema);
        public IXmlTemplateNode ResponseNode => new XRoadXmlSchemaTemplateNode(_responseElement, _schema);

        public XRoadXmlSchemaTemplate(XmlSchema schema, string elementName)
        {
            _schema = schema;

            _requestElement = schema.Items
                                   .OfType<XmlSchemaElement>()
                                   .Single(e => e.Name == elementName);

            _responseElement = schema.Items
                                    .OfType<XmlSchemaElement>()
                                    .Single(e => e.Name == elementName);
        }

        private class XRoadXmlSchemaTemplateNode : IXmlTemplateNode
        {
            private static readonly XmlQualifiedName ArrayTypeName = new("Array", NamespaceConstants.SoapEnc);

            private readonly XmlSchema _schema;
            private readonly XmlSchemaElement _nodeElement;
            private readonly IDictionary<string, XmlSchemaElement> _childElements = new Dictionary<string, XmlSchemaElement>();

            public IXmlTemplateNode this[string childNodeName, uint version] =>
                _childElements.TryGetValue(childNodeName, out var childElement)
                    ? new XRoadXmlSchemaTemplateNode(childElement, _schema)
                    : null;

            public bool IsRequired => !_nodeElement.IsNillable;
            public string Name => _nodeElement.Name;

            public IEnumerable<string> ChildNames => _childElements.Keys;

            public XRoadXmlSchemaTemplateNode(XmlSchemaElement element, XmlSchema schema)
            {
                _schema = schema;

                _nodeElement = element;

                if (element.SchemaType is XmlSchemaComplexType complexType)
                {
                    var complexContent = complexType.ContentModel as XmlSchemaComplexContent;
                    if (complexContent?.Content is XmlSchemaComplexContentRestriction restriction && restriction.BaseTypeName == ArrayTypeName)
                        AddChildElements(((XmlSchemaSequence)restriction.Particle).Items.Cast<XmlSchemaElement>().Single());
                }

                AddChildElements(element);
            }

            public int CountRequiredNodes(uint version)
            {
                return 0;
            }

            private void AddChildElements(XmlSchemaElement element)
            {
                if (element.SchemaTypeName == null || element.SchemaTypeName.Namespace != _schema.TargetNamespace)
                    return;

                var schemaType = _schema.Items
                                       .OfType<XmlSchemaType>()
                                       .Single(tp => tp.Name == element.SchemaTypeName.Name);

                if (!(schemaType is XmlSchemaComplexType complexType))
                    return;

                switch (complexType.Particle)
                {
                    case XmlSchemaChoice choice:
                        foreach (var childElement in choice.Items.Cast<XmlSchemaSequence>().SelectMany(s => s.Items.Cast<XmlSchemaElement>()))
                            _childElements.Add(childElement.Name, childElement);
                        break;

                    case XmlSchemaSequence sequence:
                        foreach (var childElement in sequence.Items.Cast<XmlSchemaElement>())
                            _childElements.Add(childElement.Name, childElement);
                        break;
                }
            }

            public string Namespace => _schema.TargetNamespace;
        }
    }
}