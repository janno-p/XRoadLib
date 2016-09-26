using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;

#if NETSTANDARD1_5
using XRoadLib.Xml.Schema;
#else
using System.Xml.Schema;
#endif

namespace XRoadLib.Styles
{
    /// <summary>
    /// X-Road message serialization style.
    /// </summary>
    public abstract class Style
    {
        /// <summary>
        /// Use this instance to create XmlNodes.
        /// </summary>
        protected readonly XmlDocument document = new XmlDocument();

        /// <summary>
        /// Writes explicit type attribute if style requires it.
        /// </summary>
        public virtual void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        { }

        /// <summary>
        /// Writes explicit array type attribute if style requires it.
        /// </summary>
        public virtual void WriteExplicitArrayType(XmlWriter writer, XName itemQualifiedName, int arraySize)
        { }

        /// <summary>
        /// Writes element type attribute according to style preferences.
        /// </summary>
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

        /// <summary>
        /// Serializes X-Road SOAP message header element.
        /// </summary>
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

        /// <summary>
        /// Create operation binding for current style.
        /// </summary>
        public abstract SoapOperationBinding CreateSoapOperationBinding();

        /// <summary>
        /// Create soap body binding for current style.
        /// </summary>
        public abstract SoapBodyBinding CreateSoapBodyBinding(string targetNamespace);

        /// <summary>
        /// Create header binding binding for current style.
        /// </summary>
        public abstract SoapHeaderBinding CreateSoapHeaderBinding(XName headerName, string messageName, string targetNamespace);

#if !NETSTANDARD1_5
        /// <summary>
        /// Create SOAP header element.
        /// </summary>
        public abstract XmlElement CreateSoapHeader(SoapHeaderBinding binding);
#endif

        /// <summary>
        /// Add expected content type attribute for binary content.
        /// </summary>
        public virtual XmlAttribute CreateExpectedContentType(string contentType)
        {
            var attribute = document.CreateAttribute(PrefixConstants.XMIME, "expectedContentTypes", NamespaceConstants.XMIME);
            attribute.Value = contentType;
            return attribute;
        }

        /// <summary>
        /// Create array item element for collection type.
        /// </summary>
        public abstract void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, Action<string> addSchemaImport);

        /// <summary>
        /// Create soap binding element for current style.
        /// </summary>
        public abstract SoapBinding CreateSoapBinding();

        /// <summary>
        /// Should message definitions use type or element references.
        /// </summary>
        public virtual bool UseElementInMessagePart => true;

        /// <summary>
        /// Preferred string serialization mode.
        /// </summary>
        public virtual StringSerializationMode StringSerializationMode => StringSerializationMode.HtmlEncoded;

        /// <summary>
        /// Serializes beginning of SOAP envelope into X-Road message.
        /// </summary>
        public virtual void WriteSoapEnvelope(XmlWriter writer, string producerNamespace)
        {
            writer.WriteStartElement(PrefixConstants.SOAP_ENV, "Envelope", NamespaceConstants.SOAP_ENV);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.SOAP_ENV, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENV);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.XSD, NamespaceConstants.XMLNS, NamespaceConstants.XSD);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.XSI, NamespaceConstants.XMLNS, NamespaceConstants.XSI);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.TARGET, NamespaceConstants.XMLNS, producerNamespace);
        }

        /// <summary>
        /// Serializes header of SOAP message.
        /// </summary>
        public virtual void WriteSoapHeader(XmlWriter writer, IXRoadHeader header, IEnumerable<XElement> additionalHeaders = null)
        {
            writer.WriteStartElement("Header", NamespaceConstants.SOAP_ENV);

            header?.WriteTo(writer);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            writer.WriteEndElement();
        }
    }
}