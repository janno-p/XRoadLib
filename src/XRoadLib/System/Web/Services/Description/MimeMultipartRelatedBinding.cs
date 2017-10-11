#if NETSTANDARD1_6

using System.Collections.Generic;
using System.Xml;
using XRoadLib;

namespace System.Web.Services.Description
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

#endif