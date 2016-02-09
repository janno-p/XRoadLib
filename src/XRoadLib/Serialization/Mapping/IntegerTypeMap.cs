using System;
using System.Numerics;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class IntegerTypeMap : TypeMap<BigInteger>
    {
        public IntegerTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : new BigInteger(XmlConvert.ToDecimal(value));
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteValue(value.ToString());
        }
    }
}