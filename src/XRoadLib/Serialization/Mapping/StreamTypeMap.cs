using System;
using System.IO;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class StreamTypeMap : TypeMap<XRoadAttachment>
    {
        private readonly XmlQualifiedName qualifiedName;

        public StreamTypeMap(XmlQualifiedName qualifiedName)
        {
            this.qualifiedName = qualifiedName;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (context.Protocol != XRoadProtocol.Version20 && !reader.ReadToDescendant("Include", NamespaceConstants.XOP))
                throw XRoadException.InvalidQuery("Päringu xml-is puudub viide (`xop:Include` element) faili sisule.");

            var contentID = reader.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(contentID))
            {
                if (context.Protocol != XRoadProtocol.Version20)
                    throw XRoadException.InvalidQuery("Päringu xml-is puudub viide (`xop:Include` elemendi `href` atribuut) faili sisule.");

                var tempAttachment = new XRoadAttachment(new MemoryStream()) { IsMultipartContent = false };
                context.AttachmentManager.AllAttachments.Add(tempAttachment);

                if (reader.IsEmptyElement || !reader.Read())
                    return tempAttachment.ContentStream;

                const int bufferSize = 1000;

                int bytesRead;
                var buffer = new byte[bufferSize];

                while ((bytesRead = reader.ReadContentAsBase64(buffer, 0, bufferSize)) > 0)
                    tempAttachment.ContentStream.Write(buffer, 0, bytesRead);

                return tempAttachment.ContentStream;
            }

            var attachment = context.AttachmentManager.GetAttachment(contentID.Substring(4));
            if (attachment == null)
                throw XRoadException.PäringusPuudubAttachment(contentID);

            return attachment.ContentStream;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            var attachment = new XRoadAttachment((Stream)value);
            context.AttachmentManager.AllAttachments.Add(attachment);

            if (context.Protocol == XRoadProtocol.Version20)
                writer.WriteTypeAttribute(qualifiedName);

            if (context.Protocol == XRoadProtocol.Version20 && !context.IsMultipart)
            {
                attachment.IsMultipartContent = false;
                attachment.WriteAsBase64(writer);
                return;
            }

            if (context.Protocol != XRoadProtocol.Version20)
            {
                writer.WriteStartElement(PrefixConstants.XOP, "Include", NamespaceConstants.XOP);
                writer.WriteAttributeString(PrefixConstants.XMIME, "contentType", NamespaceConstants.XMIME, "application/octet-stream");
            }

            writer.WriteAttributeString("href", $"cid:{attachment.ContentID}");

            if (context.Protocol != XRoadProtocol.Version20)
                writer.WriteEndElement();
        }
    }
}
