#if NETSTANDARD1_5

using System.Collections.Generic;

namespace System.Web.Services.Description
{
    public class MimePart : ServiceDescriptionFormatExtension
    {
        public IList<ServiceDescriptionFormatExtension> Extensions { get; } = new List<ServiceDescriptionFormatExtension>();
    }
}

#endif