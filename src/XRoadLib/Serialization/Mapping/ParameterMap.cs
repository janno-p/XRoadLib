using System.Reflection;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ParameterMap : IParameterMap
    {
        private readonly ISerializerCache serializerCache;
        private readonly ITypeMap defaultTypeMap;
        private readonly bool isRequired;

        public string Name { get; }
        public ParameterInfo ParameterInfo { get; }

        public ParameterMap(ISerializerCache serializerCache, string name, ParameterInfo parameterInfo, ITypeMap defaultTypeMap, bool isRequired)
        {
            Name = name;
            ParameterInfo = parameterInfo;

            this.serializerCache = serializerCache;
            this.defaultTypeMap = defaultTypeMap;
            this.isRequired = isRequired;
        }

        public bool TryDeserialize(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context, out object value)
        {
            value = null;
            if (!reader.MoveToElement(4))
                return false;

            if (reader.LocalName != Name)
            {
                if (!isRequired)
                    return false;
                throw XRoadException.InvalidQuery($"Oodati elementi `{Name}`, aga leiti `{reader.LocalName}`.");
            }

            value = DeserializeRoot(reader, parameterNode, context);

            return true;
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