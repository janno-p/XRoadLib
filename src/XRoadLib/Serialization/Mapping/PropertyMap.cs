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
        private readonly ISet<string> _filters = new HashSet<string>();
        private readonly ISerializer _serializer;
        private readonly ITypeMap _typeMap;
        private readonly GetValueMethod _getValueMethod;
        private readonly SetValueMethod _setValueMethod;

        public PropertyDefinition Definition { get; }

        public PropertyMap(ISerializer serializer, PropertyDefinition propertyDefinition, ITypeMap typeMap, IEnumerable<string> availableFilters)
        {
            _serializer = serializer;

            _typeMap = typeMap is IContentTypeMap contentTypeMap && propertyDefinition.Content.UseXop
                ? contentTypeMap.GetOptimizedContentTypeMap()
                : typeMap;

            Definition = propertyDefinition;

            _getValueMethod = Definition.PropertyInfo.CreateGetValueMethod();
            _setValueMethod = Definition.PropertyInfo.CreateSetValueMethod();

            if (availableFilters == null)
                return;

            foreach (var availableFilter in availableFilters.Where(f => Definition.DeclaringTypeDefinition.Type.IsFilterableField(Definition.RuntimeName, f)))
                _filters.Add(availableFilter);
        }

        public bool Deserialize(XmlReader reader, IXRoadSerializable dtoObject, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            if (message.EnableFiltering && !_filters.Contains(message.FilterName))
            {
                reader.ConsumeUnusedElement();
                return false;
            }

            XName typeAttribute;
            if (_typeMap.Definition.IsAnonymous && !(_typeMap is IArrayTypeMap) && (typeAttribute = reader.GetTypeAttributeValue()) != null)
                throw new UnknownTypeException($"Expected anonymous type, but `{typeAttribute}` was given.", Definition, _typeMap.Definition, typeAttribute);

            var concreteTypeMap = (_typeMap.Definition.IsInheritable ? _serializer.GetTypeMapFromXsiType(reader, Definition) : null) ?? _typeMap;

            var propertyValue = concreteTypeMap.Deserialize(reader, templateNode, Definition.Content, message);
            if (propertyValue == null)
                return true;

            _setValueMethod(dtoObject, propertyValue);

            return true;
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, XRoadMessage message)
        {
            if (message.EnableFiltering && !_filters.Contains(message.FilterName))
                return;

            var propertyValue = value != null ? _getValueMethod(value) : null;

            if (Definition.Content.IsOptional && !Definition.Content.IsNullable && propertyValue == null)
                return;

            if (!Definition.Content.MergeContent)
            {
                writer.WriteStartElement(Definition.Content.Name);

                if (propertyValue == null)
                    writer.WriteNilAttribute();
            }

            if (propertyValue != null)
            {
                var concreteTypeMap = _typeMap.Definition.IsInheritable ? _serializer.GetTypeMap(propertyValue.GetType()) : _typeMap;
                concreteTypeMap.Serialize(writer, templateNode, propertyValue, Definition.Content, message);
            }

            if (!Definition.Content.MergeContent)
                writer.WriteEndElement();
        }
    }
}