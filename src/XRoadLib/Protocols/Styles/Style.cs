using System.Xml;
using System.Xml.Linq;

namespace XRoadLib.Protocols.Styles
{
    public abstract class Style
    {
        public virtual void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        { }

        public virtual void WriteExplicitArrayType(XmlWriter writer, XName itemQualifiedName, int arraySize)
        { }
    }
}