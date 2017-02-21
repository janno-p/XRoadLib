#if NETSTANDARD1_6_1

using System.Collections.Generic;
using System.Xml;
using XRoadLib;

namespace System.Web.Services.Description
{
    public class MimePart : ServiceDescriptionFormatExtension
    {
        public List<ServiceDescriptionFormatExtension> Extensions { get; } = new List<ServiceDescriptionFormatExtension>();

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.MIME, "part", NamespaceConstants.MIME);
            Extensions.ForEach(x => x.Write(writer));
            writer.WriteEndElement();
        }
    }
}

#endif