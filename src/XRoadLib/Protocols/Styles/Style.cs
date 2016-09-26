using System;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;

#if NETSTANDARD1_5
using XRoadLib.Xml.Schema;
#else
using System.Xml.Schema;
#endif

namespace XRoadLib.Protocols.Styles
{
    public abstract class Style
    {
        protected readonly XmlDocument document = new XmlDocument();

        public virtual void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        { }

        public virtual void WriteExplicitArrayType(XmlWriter writer, XName itemQualifiedName, int arraySize)
        { }

        public virtual void WriteType(XmlWriter writer, TypeDefinition typeDefinition, Type expectedType, bool disableExplicitType)
        {
            if (typeDefinition.IsAnonymous)
                return;

            if (typeDefinition.Type != expectedType)
            {
                writer.WriteTypeAttribute(typeDefinition.Name);
                return;
            }

            if (!disableExplicitType)
                WriteExplicitType(writer, typeDefinition.Name);
        }

        public void WriteHeaderElement(XmlWriter writer, string name, object value, XName typeName)
        {
            writer.WriteStartElement(name, NamespaceConstants.XTEE);

            WriteExplicitType(writer, typeName);

            var stringValue = value as string;
            if (stringValue != null)
                writer.WriteStringWithMode(stringValue, StringSerializationMode);
            else writer.WriteValue(value);

            writer.WriteEndElement();
        }

        public abstract SoapOperationBinding CreateSoapOperationBinding();

        public abstract SoapBodyBinding CreateSoapBodyBinding(string targetNamespace);

        public abstract SoapHeaderBinding CreateSoapHeaderBinding(XName headerName, string messageName, string targetNamespace);

#if !NETSTANDARD1_5
        public abstract XmlElement CreateSoapHeader(SoapHeaderBinding binding);
#endif

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

        public abstract void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, Action<string> addSchemaImport);

        public abstract SoapBinding CreateSoapBinding();

        public virtual bool UseElementInMessagePart => true;

        public virtual StringSerializationMode StringSerializationMode => StringSerializationMode.HtmlEncoded;
    }
}