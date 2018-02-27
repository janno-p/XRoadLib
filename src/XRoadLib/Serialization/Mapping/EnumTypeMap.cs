using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class EnumTypeMap : TypeMap
    {
        private readonly IDictionary<string, int> deserializationMapping = new Dictionary<string, int>();
        private readonly IDictionary<int, string> serializationMapping = new Dictionary<int, string>();

        public EnumTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            foreach (var name in Enum.GetNames(typeDefinition.Type))
            {
                var memberInfo = typeDefinition.Type.GetTypeInfo().GetMember(name).Single();
                var attribute = memberInfo.GetSingleAttribute<XmlEnumAttribute>();
                var value = (attribute?.Name).GetValueOrDefault(name);
                var enumValue = (int)Enum.Parse(typeDefinition.Type, name);
                deserializationMapping.Add(value, enumValue);
                serializationMapping.Add(enumValue, value);
            }
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var isEmptyElement = reader.IsEmptyElement;

            var stringValue = isEmptyElement ? "" : reader.ReadElementContentAsString();

            if (!deserializationMapping.TryGetValue(stringValue, out var enumerationValue))
                throw new MissingFieldException($"Unexpected value `{stringValue}` for enumeration type `{Definition.Name}`.");

            var result = Enum.ToObject(Definition.Type, enumerationValue);

            return isEmptyElement ? MoveNextAndReturn(reader, result) : result;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            if (!(content.Particle is RequestDefinition))
                message.Style.WriteExplicitType(writer, Definition.Name);

            if (!serializationMapping.TryGetValue((int)value, out var enumerationValue))
                throw new MissingFieldException($"Cannot map value `{value}` to enumeration type `{Definition.Name}`.");

            writer.WriteValue(enumerationValue);
        }
    }
}