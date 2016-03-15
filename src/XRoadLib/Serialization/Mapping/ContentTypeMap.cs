using System.IO;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ContentTypeMap : TypeMap, IContentTypeMap
    {
        private readonly XName encodedTypeName;
        private readonly ITypeMap optimizedContentTypeMap;

        public ContentTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            encodedTypeName = XName.Get(Definition.Name.LocalName, NamespaceConstants.SOAP_ENC);
            optimizedContentTypeMap = new OptimizedContentTypeMap(this);
        }

        public ITypeMap GetOptimizedContentTypeMap()
        {
            return optimizedContentTypeMap;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            var contentID = reader.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(contentID))
            {
                if (message.IsMultipartContainer)
                    throw XRoadException.InvalidQuery("Missing `href` attribute to multipart content.");

                var tempAttachment = new XRoadAttachment(new MemoryStream()) { IsMultipartContent = false };
                message.AllAttachments.Add(tempAttachment);

                if (reader.IsEmptyElement)
                    return MoveNextAndReturn(reader, tempAttachment.ContentStream);

                reader.Read();

                const int bufferSize = 1000;

                int bytesRead;
                var buffer = new byte[bufferSize];

                while ((bytesRead = reader.ReadContentAsBase64(buffer, 0, bufferSize)) > 0)
                    tempAttachment.ContentStream.Write(buffer, 0, bytesRead);

                return tempAttachment.ContentStream;
            }

            var attachment = message.GetAttachment(contentID.Substring(4));
            if (attachment == null)
                throw XRoadException.PäringusPuudubAttachment(contentID);

            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, attachment.ContentStream);

            reader.ReadToEndElement();

            return attachment.ContentStream;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message)
        {
            var attachment = new XRoadAttachment((Stream)value);
            message.AllAttachments.Add(attachment);

            if (message.BinaryMode == BinaryMode.Attachment)
            {
                if (!(definition is RequestValueDefinition))
                    message.Protocol.Style.WriteExplicitType(writer, encodedTypeName);

                writer.WriteAttributeString("href", $"cid:{attachment.ContentID}");
                return;
            }

            if (!(definition is RequestValueDefinition))
                message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            attachment.IsMultipartContent = false;
            attachment.WriteAsBase64(writer);
        }
    }
}
