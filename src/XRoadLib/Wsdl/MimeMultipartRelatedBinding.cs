using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class MimeMultipartRelatedBinding : ServiceDescriptionFormatExtension
    {
        public List<MimePart> Parts { get; } = new List<MimePart>();

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.MIME, "multipartRelated", NamespaceConstants.MIME);
            Parts.ForEach(x => x.Write(writer));
            writer.WriteEndElement();
        }
    }
}