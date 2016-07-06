#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSequence : XmlSchemaGroupBase
    {
        internal override void Write(XmlWriter writer)
        {
            WriteStartElement(writer, "sequence");
            WriteAttributes(writer);
            base.Write(writer);
            writer.WriteEndElement();
        }
    }
}

#endif