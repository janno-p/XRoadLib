using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class SequenceTypeMap<T> : CompositeTypeMap<T> where T : class, IXRoadSerializable, new()
    {
        public SequenceTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
            : base(serializerCache, typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            var entity = new T();
            entity.SetTemplateMembers(templateNode.ChildNames);

            var validateRequired = definition.Particle is RequestDefinition;

            if (contentPropertyMap != null)
            {
                ReadPropertyValue(reader, contentPropertyMap, templateNode[contentPropertyMap.Definition.TemplateName, message.Version], message, validateRequired, entity);
                return entity;
            }

            var properties = propertyMaps.GetEnumerator();

            if (reader.IsEmptyElement)
            {
                ValidateRemainingProperties(properties, definition);
                return MoveNextAndReturn(reader, entity);
            }

            var parentDepth = reader.Depth;
            var itemDepth = parentDepth + 1;

            reader.Read();

            while (parentDepth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != itemDepth)
                {
                    reader.Read();
                    continue;
                }

                var propertyNode = MoveToProperty(reader, properties, templateNode, message, validateRequired);

                ReadPropertyValue(reader, properties.Current, propertyNode, message, validateRequired, entity);
            }

            ValidateRemainingProperties(properties, definition);

            return entity;
        }

        private static void ReadPropertyValue(XmlReader reader, IPropertyMap propertyMap, IXmlTemplateNode propertyNode, XRoadMessage message, bool validateRequired, T dtoObject)
        {
            if (propertyNode == null)
            {
                reader.ConsumeUnusedElement();
                return;
            }

            var isNull = reader.IsNilElement();
            if (validateRequired && isNull && propertyNode.IsRequired)
                throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyMap.Definition.Content.Name.LocalName, 1));

            var templateName = propertyMap.Definition.TemplateName;
            if ((isNull || propertyMap.Deserialize(reader, dtoObject, propertyNode, message)) && !string.IsNullOrWhiteSpace(templateName))
                dtoObject.OnMemberDeserialized(templateName);

            reader.ConsumeNilElement(isNull);
        }

        private IXmlTemplateNode MoveToProperty(XmlReader reader, IEnumerator<IPropertyMap> properties, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            IPropertyMap propertyMap = null;

            while (properties.MoveNext())
            {
                propertyMap = properties.Current;
                var propertyName = propertyMap.Definition.Content.SerializedName;
                var propertyNode = templateNode[properties.Current.Definition.TemplateName, message.Version];

                if (reader.LocalName == propertyName)
                    return propertyNode;

                if (!propertyMap.Definition.Content.IsOptional)
                    throw XRoadException.UnexpectedElementInQuery(Definition.Name, propertyName, reader.LocalName);

                if (validateRequired && propertyNode != null && propertyNode.IsRequired)
                    throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyName, 1));
            }

            if (propertyMap == null)
                throw XRoadException.InvalidQuery($"Element `{reader.LocalName}` was unexpected at given location while deserializing type `{Definition.Name}`.");

            throw XRoadException.UnexpectedElementInQuery(Definition.Name, propertyMap.Definition.Content.SerializedName, reader.LocalName);
        }

        private void ValidateRemainingProperties(IEnumerator<IPropertyMap> properties, IContentDefinition contentDefinition)
        {
            while (properties.MoveNext())
                if (!properties.Current.Definition.Content.IsOptional)
                {
                    var typeName = Definition?.Name ?? ((contentDefinition as ArrayItemDefinition)?.WrapperDefinition.Particle as PropertyDefinition)?.DeclaringTypeDefinition?.Name;
                    throw XRoadException.InvalidQuery($"Element `{properties.Current.Definition.Content.SerializedName}` is required by type `{typeName}` definition.");
                }
        }
    }
}