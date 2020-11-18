using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class EnumTypeMap : TypeMap
    {
        private readonly IDictionary<string, int> _deserializationMapping = new Dictionary<string, int>();
        private readonly IDictionary<int, string> _serializationMapping = new Dictionary<int, string>();

        public EnumTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            foreach (var name in Enum.GetNames(typeDefinition.Type))
            {
                var memberInfo = typeDefinition.Type.GetTypeInfo().GetMember(name).Single();
                var attribute = memberInfo.GetSingleAttribute<XmlEnumAttribute>();
                var value = (attribute?.Name).GetValueOrDefault(name);
                var enumValue = (int)Enum.Parse(typeDefinition.Type, name);
                _deserializationMapping.Add(value, enumValue);
                _serializationMapping.Add(enumValue, value);
            }
        }

        public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var isEmptyElement = reader.IsEmptyElement;

            var stringValue = isEmptyElement ? "" : await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            if (!_deserializationMapping.TryGetValue(stringValue, out var enumerationValue))
                throw new UnexpectedValueException($"Unexpected value `{stringValue}` for enumeration type `{Definition.Name}`.", Definition, stringValue);

            var result = Enum.ToObject(Definition.Type, enumerationValue);

            return isEmptyElement ? await reader.MoveNextAndReturnAsync(result).ConfigureAwait(false) : result;
        }

        public override async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            await message.Style.WriteTypeAsync(writer, Definition, content).ConfigureAwait(false);

            if (!_serializationMapping.TryGetValue((int)value, out var enumerationValue))
                throw new UnexpectedValueException($"Cannot map value `{value}` to enumeration type `{Definition.Name}`.", Definition, value);

            await writer.WriteStringAsync(enumerationValue).ConfigureAwait(false);
        }
    }
}