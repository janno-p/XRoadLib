using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    /// <summary>
    /// Serialization/deserialization details of `string` type.
    /// </summary>
    public class StringTypeMap : TypeMap
    {
        /// <summary>
        /// Initializes new string type map.
        /// </summary>
        public StringTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        /// <summary>
        /// String deserialization logic.
        /// </summary>
        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, null);

            var value = reader.ReadElementContentAsString();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// String serialization logic.
        /// </summary>
        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            if (!(content.Particle is RequestDefinition))
                message.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteStringWithMode(value.ToString(), message.Style.StringSerializationMode);
        }
    }
}