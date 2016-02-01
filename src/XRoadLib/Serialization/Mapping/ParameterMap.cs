using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ParameterMap : IParameterMap
    {
        private readonly ISerializerCache serializerCache;
        private readonly ITypeMap typeMap;

        public ParameterDefinition Definition { get; }

        public ParameterMap(ISerializerCache serializerCache, ParameterDefinition parameterDefinition, ITypeMap typeMap)
        {
            this.serializerCache = serializerCache;
            this.typeMap = typeMap;

            Definition = parameterDefinition;
        }

        public bool TryDeserialize(XmlReader reader, IXmlTemplateNode parameterNode, XRoadMessage message, out object value)
        {
            value = null;
            if (!reader.MoveToElement(4))
                return false;

            if (reader.LocalName != Definition.Name.LocalName)
            {
                if (Definition.IsOptional)
                    return false;
                throw XRoadException.InvalidQuery($"Oodati elementi `{Definition.Name.LocalName}`, aga leiti `{reader.LocalName}`.");
            }

            value = DeserializeRoot(reader, parameterNode, message);

            return true;
        }

        public object DeserializeRoot(XmlReader reader, IXmlTemplateNode parameterNode, XRoadMessage message)
        {
            if (parameterNode == null || reader.IsNilElement())
            {
                reader.ReadToEndElement();
                return null;
            }

            var currentTypeMap = serializerCache.GetTypeMapFromXsiType(reader) ?? typeMap;

            return currentTypeMap.Deserialize(reader, parameterNode, message);
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode parameterNode, object value, XRoadMessage message)
        {
            if (Definition.Name != null)
                writer.WriteStartElement(Definition.Name.LocalName);

            SerializeRoot(writer, parameterNode, value, message);

            if (Definition.Name != null)
                writer.WriteEndElement();
        }

        public void SerializeRoot(XmlWriter writer, IXmlTemplateNode parameterNode, object value, XRoadMessage message)
        {
            if (value == null)
                writer.WriteNilAttribute();
            else
            {
                var currentTypeMap = typeMap.Definition.IsSimpleType ? typeMap : serializerCache.GetTypeMap(value.GetType());
                currentTypeMap.Serialize(writer, parameterNode, value, typeMap.Definition.Type, message);
            }
        }
    }
}