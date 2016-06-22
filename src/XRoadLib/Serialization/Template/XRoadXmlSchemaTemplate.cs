using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

#if NETSTANDARD1_5
using XRoadLib.Xml.Schema;
#else
using System.Xml.Schema;
#endif

namespace XRoadLib.Serialization.Template
{
    public class XRoadXmlSchemaTemplate : IXmlTemplate
    {
        private readonly XmlSchema schema;
        private readonly XmlSchemaElement requestElement;
        private readonly XmlSchemaElement responseElement;

        public IDictionary<string, Type> ParameterTypes { get { throw new NotImplementedException(); } }
        public IEnumerable<IXmlTemplateNode> ParameterNodes { get { throw new NotImplementedException(); } }
        public IXmlTemplateNode RequestNode => new XRoadXmlSchemaTemplateNode(requestElement, schema);
        public IXmlTemplateNode ResponseNode => new XRoadXmlSchemaTemplateNode(responseElement, schema);

        public XRoadXmlSchemaTemplate(XmlSchema schema, string elementName)
        {
            this.schema = schema;

            requestElement = schema.Items
                                   .OfType<XmlSchemaElement>()
                                   .Single(e => e.Name == elementName);

            responseElement = schema.Items
                                    .OfType<XmlSchemaElement>()
                                    .Single(e => e.Name == elementName);
        }

        public IXmlTemplateNode GetParameterNode(string parameterName)
        {
            throw new NotImplementedException();
        }

        private class XRoadXmlSchemaTemplateNode : IXmlTemplateNode
        {
            private static readonly XmlQualifiedName arrayTypeName = new XmlQualifiedName("Array", NamespaceConstants.SOAP_ENC);

            private readonly XmlSchema schema;
            private readonly XmlSchemaElement nodeElement;
            private readonly IDictionary<string, XmlSchemaElement> childElements = new Dictionary<string, XmlSchemaElement>();

            public IXmlTemplateNode this[string childNodeName, uint version]
            {
                get
                {
                    XmlSchemaElement childElement;
                    return childElements.TryGetValue(childNodeName, out childElement)
                        ? new XRoadXmlSchemaTemplateNode(childElement, schema)
                        : null;
                }
            }

            public bool IsRequired => !nodeElement.IsNillable;
            public string Name => nodeElement.Name;

            public IEnumerable<string> ChildNames => childElements.Keys;

            public XRoadXmlSchemaTemplateNode(XmlSchemaElement element, XmlSchema schema)
            {
                this.schema = schema;

                nodeElement = element;

                var complexType = element.SchemaType as XmlSchemaComplexType;
                if (complexType != null)
                {
                    var complexContent = complexType.ContentModel as XmlSchemaComplexContent;
                    var restriction = complexContent?.Content as XmlSchemaComplexContentRestriction;
                    if (restriction != null && restriction.BaseTypeName == arrayTypeName)
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
                if (element.SchemaTypeName == null || element.SchemaTypeName.Namespace != schema.TargetNamespace)
                    return;

                var schemaType = schema.Items
                                       .OfType<XmlSchemaType>()
                                       .Single(tp => tp.Name == element.SchemaTypeName.Name);

                var complexType = schemaType as XmlSchemaComplexType;
                if (complexType == null)
                    return;

                var choice = complexType.Particle as XmlSchemaChoice;
                if (choice != null)
                    foreach (var childElement in choice.Items.Cast<XmlSchemaSequence>().SelectMany(s => s.Items.Cast<XmlSchemaElement>()))
                        childElements.Add(childElement.Name, childElement);

                var sequence = complexType.Particle as XmlSchemaSequence;
                if (sequence != null)
                    foreach (var childElement in sequence.Items.Cast<XmlSchemaElement>())
                        childElements.Add(childElement.Name, childElement);
            }

            public string Namespace => schema.TargetNamespace;
        }
    }
}