using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Protocols
{
    public class RpcEncodedStyle : Style
    {
        public override void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        {
            writer.WriteTypeAttribute(qualifiedName);
        }
    }
}