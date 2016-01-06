using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ParameterMap : IParameterMap
    {
        private readonly ISerializerCache serializerCache;
        private readonly ITypeMap defaultTypeMap;
        private readonly bool isOptional;

        public string Name { get; }

        public ParameterMap(ISerializerCache serializerCache, string name, ITypeMap defaultTypeMap, bool isOptional)
        {
            Name = name;

            this.serializerCache = serializerCache;
            this.defaultTypeMap = defaultTypeMap;
            this.isOptional = isOptional;
        }

        public object Deserialize(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context)
        {
            if (!reader.MoveToElement(4))
                return null;

            if (reader.LocalName != Name && !isOptional)
                throw XRoadException.ViganePäring($"Oodati elementi `{Name}`, aga leiti `{reader.LocalName}`.");

            return DeserializeRoot(reader, parameterNode, context);
        }

        public object DeserializeRoot(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context)
        {
            if (parameterNode == null || reader.IsNilElement())
            {
                reader.ReadToEndElement();
                return null;
            }

            var typeMap = serializerCache.GetTypeMapFromXsiType(reader, context.DtoVersion) ?? defaultTypeMap;

            return typeMap.Deserialize(reader, parameterNode, context);
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context)
        {
            if (!string.IsNullOrWhiteSpace(Name))
                writer.WriteStartElement(Name);

            SerializeRoot(writer, parameterNode, value, context);

            if (!string.IsNullOrWhiteSpace(Name))
                writer.WriteEndElement();
        }

        public void SerializeRoot(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context)
        {
            if (value == null)
                writer.WriteNilAttribute();
            else
            {
                var typeMap = defaultTypeMap.IsSimpleType ? defaultTypeMap : serializerCache.GetTypeMap(value.GetType(), context.DtoVersion);
                typeMap.Serialize(writer, parameterNode, value, defaultTypeMap.RuntimeType, context);
            }
        }
    }
}