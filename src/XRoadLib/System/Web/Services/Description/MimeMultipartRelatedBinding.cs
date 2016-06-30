#if NETSTANDARD1_5

using System.Collections.Generic;

namespace System.Web.Services.Description
{
    public class MimeMultipartRelatedBinding : ServiceDescriptionFormatExtension
    {
        public List<MimePart> Parts { get; } = new List<MimePart>();
    }
}

#endif