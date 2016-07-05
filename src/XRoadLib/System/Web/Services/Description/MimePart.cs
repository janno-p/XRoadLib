#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class MimePart : ServiceDescriptionFormatExtension
    {
        public List<ServiceDescriptionFormatExtension> Extensions { get; } = new List<ServiceDescriptionFormatExtension>();

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif