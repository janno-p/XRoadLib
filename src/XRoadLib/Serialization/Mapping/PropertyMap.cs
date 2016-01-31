using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class PropertyMap : IPropertyMap
    {
        private readonly bool isFilterable;
        private readonly ISerializerCache serializerCache;
        private readonly ITypeMap typeMap;
        private readonly GetValueMethod getValueMethod;
        private readonly SetValueMethod setValueMethod;

        public PropertyDefinition Definition { get; }

        public PropertyMap(ISerializerCache serializerCache, PropertyDefinition propertyDefinition, ITypeMap typeMap)
        {
            this.serializerCache = serializerCache;
            this.typeMap = typeMap;

            Definition = propertyDefinition;

            getValueMethod = Definition.PropertyInfo.CreateGetValueMethod();
            setValueMethod = Definition.PropertyInfo.CreateSetValueMethod();
            isFilterable = Definition.Owner.Type.IsFilterableField(Definition.Name.LocalName);
        }

        public bool Deserialize(XmlReader reader, IXRoadSerializable dtoObject, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            if (message.EnableFiltering && !isFilterable)
            {
                reader.ReadToEndElement();
                return false;
            }

            string typeAttribute;
            if (typeMap.Definition.IsAnonymous && (typeAttribute = reader.GetAttribute("type", NamespaceConstants.XSI)) != null)
                throw XRoadException.InvalidQuery("Expected anonymous type, but `{0}` was given.", typeAttribute);

            var concreteTypeMap = (typeMap.Definition.IsInheritable ? serializerCache.GetTypeMapFromXsiType(reader) : null) ?? typeMap;

            var propertyValue = concreteTypeMap.Deserialize(reader, templateNode, message);
            if (propertyValue == null)
                return true;

            setValueMethod(dtoObject, propertyValue);

            return true;
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, XRoadMessage message)
        {
            if (message.EnableFiltering && !isFilterable)
                return;

            var propertyValue = getValueMethod(value);

            writer.WriteStartElement(Definition.Name.LocalName);

            if (propertyValue == null)
                writer.WriteNilAttribute();
            else
            {
                var concreteTypeMap = typeMap.Definition.IsInheritable ? serializerCache.GetTypeMap(propertyValue.GetType()) : typeMap;

                concreteTypeMap.Serialize(writer, templateNode, propertyValue, typeMap.Definition.Type, message);
            }

            writer.WriteEndElement();
        }
    }
}