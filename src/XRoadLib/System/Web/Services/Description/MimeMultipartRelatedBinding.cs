#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class MimeMultipartRelatedBinding : ServiceDescriptionFormatExtension
    {
        public List<MimePart> Parts { get; } = new List<MimePart>();

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif