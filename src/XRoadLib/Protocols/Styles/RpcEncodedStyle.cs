using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Protocols.Styles
{
    public class RpcEncodedStyle : Style
    {
        public override void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        {
            writer.WriteTypeAttribute(qualifiedName);
        }

        public override void WriteExplicitArrayType(XmlWriter writer, XName itemQualifiedName, int arraySize)
        {
            writer.WriteTypeAttribute("Array", NamespaceConstants.SOAP_ENC);
            writer.WriteArrayTypeAttribute(itemQualifiedName, arraySize);
        }
    }
}