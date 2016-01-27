using System.Xml;
using System.Xml.Linq;

namespace XRoadLib.Protocols
{
    public abstract class Style
    {
        public virtual void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        { }
    }
}