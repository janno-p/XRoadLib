using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class MimePart : ServiceDescriptionFormatExtension
    {
        public List<ServiceDescriptionFormatExtension> Extensions { get; } = new List<ServiceDescriptionFormatExtension>();

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.Mime, "part", NamespaceConstants.Mime);
            Extensions.ForEach(x => x.Write(writer));
            writer.WriteEndElement();
        }
    }
}