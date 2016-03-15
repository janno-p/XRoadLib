using System.IO;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class OptimizedContentTypeMap : ITypeMap
    {
        public TypeDefinition Definition { get; }

        public OptimizedContentTypeMap(ContentTypeMap contentTypeMap)
        {
            Definition = contentTypeMap.Definition;
        }

        public object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            if (!reader.ReadToDescendant("Include", NamespaceConstants.XOP))
                throw XRoadException.InvalidQuery("Missing `xop:Include` reference to multipart content.");

            var contentID = reader.GetAttribute("href");
            if (string.IsNullOrWhiteSpace(contentID))
                throw XRoadException.InvalidQuery("Missing `href` attribute to multipart content.");

            var attachment = message.GetAttachment(contentID.Substring(4));
            if (attachment == null)
                throw XRoadException.PäringusPuudubAttachment(contentID);

            return attachment.ContentStream;
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message)
        {
            var attachment = new XRoadAttachment((Stream)value);
            message.AllAttachments.Add(attachment);

            if (!(definition is RequestValueDefinition))
                message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteStartElement(PrefixConstants.XOP, "Include", NamespaceConstants.XOP);
            //writer.WriteAttributeString(PrefixConstants.XMIME, "contentType", NamespaceConstants.XMIME, "application/octet-stream");

            writer.WriteAttributeString("href", $"cid:{attachment.ContentID}");

            writer.WriteEndElement();
        }
    }
}