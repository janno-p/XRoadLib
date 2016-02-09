using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class SequenceTypeMap<T> : TypeMap<T> where T : class, IXRoadSerializable, new()
    {
        private readonly ISerializerCache serializerCache;
        private readonly IList<IPropertyMap> propertyMaps = new List<IPropertyMap>();

        private IPropertyMap mergePropertyMap;

        public SequenceTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            this.serializerCache = serializerCache;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            var entity = new T();
            entity.SetTemplateMembers(templateNode.ChildNames);

            if (reader.IsEmptyElement)
                return entity;

            if (mergePropertyMap != null)
            {
                ReadPropertyValue(reader, mergePropertyMap, templateNode[mergePropertyMap.Definition.TemplateName, message.Version], message, validateRequired, entity);
                return entity;
            }

            var depth = reader.Depth;
            var properties = propertyMaps.GetEnumerator();

            while (reader.Read() && depth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                var propertyNode = MoveToProperty(reader, properties, templateNode, message, validateRequired);

                ReadPropertyValue(reader, properties.Current, propertyNode, message, validateRequired, entity);
            }

            return entity;
        }

        private static void ReadPropertyValue(XmlReader reader, IPropertyMap propertyMap, IXmlTemplateNode propertyNode, XRoadMessage message, bool validateRequired, T dtoObject)
        {
            if (propertyNode == null)
            {
                reader.ReadToEndElement();
                return;
            }

            var isNull = reader.IsNilElement();
            if (validateRequired && isNull && propertyNode.IsRequired)
                throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyMap.Definition.Name.LocalName, 1));

            if (isNull || propertyMap.Deserialize(reader, dtoObject, propertyNode, message))
                dtoObject.OnMemberDeserialized(reader.LocalName);
        }

        private IXmlTemplateNode MoveToProperty(XmlReader reader, IEnumerator<IPropertyMap> properties, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            while (properties.MoveNext())
            {
                var propertyName = properties.Current.Definition.Name.LocalName;
                var propertyNode = templateNode[properties.Current.Definition.TemplateName, message.Version];

                if (reader.LocalName == propertyName)
                    return propertyNode;

                if (validateRequired && propertyNode != null && propertyNode.IsRequired)
                    throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyName, 1));
            }

            throw XRoadException.InvalidQuery("Andmetüübil `{0}` puudub element `{1}` või see on esitatud vales kohas.", Definition.Name, reader.LocalName);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            message.Protocol.Style.WriteType(writer, Definition, expectedType);

            foreach (var propertyMap in propertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.Definition.TemplateName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, message);
            }
        }

        public override void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions, IEnumerable<string> availableFilters)
        {
            if (propertyMaps.Count > 0)
                return;

            var createdPropertyMaps = propertyDefinitions.Select(x => new PropertyMap(serializerCache, x.Item1, x.Item2, availableFilters)).ToList();
            if (createdPropertyMaps.Count == 1 && createdPropertyMaps[0].Definition.MergeContent)
            {
                mergePropertyMap = createdPropertyMaps[0];
                return;
            }

            foreach (var propertyMap in createdPropertyMaps)
            {
                if (propertyMap.Definition.MergeContent)
                    throw new Exception($"Property {propertyMap.Definition} of type {Definition} cannot be merged, because there are more than 1 properties present.");

                propertyMaps.Add(propertyMap);
            }
        }
    }
}