using System;
using System.IO;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class StreamTypeMap : TypeMap<XRoadAttachment>
    {
        public StreamTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (context.BinaryMode == BinaryMode.Xop && !reader.ReadToDescendant("Include", NamespaceConstants.XOP))
                throw XRoadException.InvalidQuery("Missing `xop:Include` reference to multipart content.");

            var contentID = reader.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(contentID))
            {
                if (context.BinaryMode != BinaryMode.Inline)
                    throw XRoadException.InvalidQuery("Missing `href` attribute to multipart content.");

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

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            var attachment = new XRoadAttachment((Stream)value);
            context.AttachmentManager.AllAttachments.Add(attachment);

            context.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            if (context.BinaryMode == BinaryMode.Inline)
            {
                attachment.IsMultipartContent = false;
                attachment.WriteAsBase64(writer);
                return;
            }

            if (context.BinaryMode == BinaryMode.Xop)
            {
                writer.WriteStartElement(PrefixConstants.XOP, "Include", NamespaceConstants.XOP);
                writer.WriteAttributeString(PrefixConstants.XMIME, "contentType", NamespaceConstants.XMIME, "application/octet-stream");
            }

            writer.WriteAttributeString("href", $"cid:{attachment.ContentID}");

            if (context.BinaryMode == BinaryMode.Xop)
                writer.WriteEndElement();
        }
    }
}
