using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class PropertyMap : IPropertyMap
    {
        private readonly ISet<string> filters = new HashSet<string>();
        private readonly ISerializer serializer;
        private readonly ITypeMap typeMap;
        private readonly GetValueMethod getValueMethod;
        private readonly SetValueMethod setValueMethod;

        public PropertyDefinition Definition { get; }

        public PropertyMap(ISerializer serializer, PropertyDefinition propertyDefinition, ITypeMap typeMap, IEnumerable<string> availableFilters)
        {
            this.serializer = serializer;

            this.typeMap = typeMap is IContentTypeMap contentTypeMap && propertyDefinition.Content.UseXop
                ? contentTypeMap.GetOptimizedContentTypeMap()
                : typeMap;

            Definition = propertyDefinition;

            getValueMethod = Definition.PropertyInfo.CreateGetValueMethod();
            setValueMethod = Definition.PropertyInfo.CreateSetValueMethod();

            if (availableFilters == null)
                return;

            foreach (var availableFilter in availableFilters.Where(f => Definition.DeclaringTypeDefinition.Type.IsFilterableField(Definition.RuntimeName, f)))
                filters.Add(availableFilter);
        }

        public bool Deserialize(XmlReader reader, IXRoadSerializable dtoObject, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            if (message.EnableFiltering && !filters.Contains(message.FilterName))
            {
                reader.ConsumeUnusedElement();
                return false;
            }

            XName typeAttribute;
            if (typeMap.Definition.IsAnonymous && !(typeMap is IArrayTypeMap) && (typeAttribute = reader.GetTypeAttributeValue()) != null)
                throw new UnknownTypeException($"Expected anonymous type, but `{typeAttribute}` was given.", Definition, typeMap.Definition, typeAttribute);

            var concreteTypeMap = (typeMap.Definition.IsInheritable ? serializer.GetTypeMapFromXsiType(reader, Definition) : null) ?? typeMap;

            var propertyValue = concreteTypeMap.Deserialize(reader, templateNode, Definition.Content, message);
            if (propertyValue == null)
                return true;

            setValueMethod(dtoObject, propertyValue);

            return true;
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, XRoadMessage message)
        {
            if (message.EnableFiltering && !filters.Contains(message.FilterName))
                return;

            var propertyValue = value != null ? getValueMethod(value) : null;

            if (Definition.Content.IsOptional && propertyValue == null)
                return;

            if (!Definition.Content.MergeContent)
            {
                writer.WriteStartElement(Definition.Content.Name);

                if (propertyValue == null)
                    writer.WriteNilAttribute();
            }

            if (propertyValue != null)
            {
                var concreteTypeMap = typeMap.Definition.IsInheritable ? serializer.GetTypeMap(propertyValue.GetType()) : typeMap;
                concreteTypeMap.Serialize(writer, templateNode, propertyValue, Definition.Content, message);
            }

            if (!Definition.Content.MergeContent)
                writer.WriteEndElement();
        }
    }
}