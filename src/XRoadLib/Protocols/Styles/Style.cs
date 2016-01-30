using System;
using System.Collections.Generic;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XRoadLib.Extensions;
using XRoadLib.Schema;

namespace XRoadLib.Protocols.Styles
{
    public abstract class Style
    {
        protected XmlDocument document = new XmlDocument();

        public virtual void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        { }

        public virtual void WriteExplicitArrayType(XmlWriter writer, XName itemQualifiedName, int arraySize)
        { }

        public virtual void WriteType(XmlWriter writer, TypeDefinition typeDefinition, Type expectedType)
        {
            if (typeDefinition.IsAnonymous)
                return;

            if (typeDefinition.Type != expectedType)
            {
                writer.WriteTypeAttribute(typeDefinition.Name);
                return;
            }

            WriteExplicitType(writer, typeDefinition.Name);
        }

        public abstract XmlElement CreateSoapHeader(SoapHeaderBinding binding);

        public virtual XmlAttribute CreateExpectedContentType(string contentType)
        {
            var attribute = document.CreateAttribute(PrefixConstants.XMIME, "expectedContentTypes", NamespaceConstants.XMIME);
            attribute.Value = contentType;
            return attribute;
        }

        public virtual XmlAttribute CreateArrayTypeAttribute(XName qualifiedName)
        {
            return null;
        }

        public abstract void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, ISet<string> requiredImports);

        public abstract SoapBinding CreateSoapBinding();
    }
}