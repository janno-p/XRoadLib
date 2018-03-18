using System;
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
        public SequenceTypeMap(ISerializer serializer, TypeDefinition typeDefinition)
            : base(serializer, typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var entity = new T();
            entity.SetTemplateMembers(templateNode.ChildNames);

            var validateRequired = content.Particle is RequestDefinition;

            if (contentPropertyMap != null)
            {
                ReadPropertyValue(reader, contentPropertyMap, templateNode[contentPropertyMap.Definition.TemplateName, message.Version], message, validateRequired, entity);
                return entity;
            }

            var properties = propertyMaps.GetEnumerator();

            if (reader.IsEmptyElement)
            {
                ValidateRemainingProperties(properties, content);
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

            ValidateRemainingProperties(properties, content);

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
                propertyMap = properties.Current ?? throw new NullReferenceException();

                var propertyName = propertyMap.Definition.Content.SerializedName.LocalName;
                var propertyNode = templateNode[properties.Current.Definition.TemplateName, message.Version];

                if (reader.LocalName == propertyName)
                    return propertyNode;

                if (!propertyMap.Definition.Content.IsOptional)
                    throw XRoadException.UnexpectedElementInQuery(Definition.Name, propertyName, reader.LocalName);

                if (validateRequired && propertyNode != null && propertyNode.IsRequired)
                    throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyName, 1));
            }

            if (propertyMap == null)
                throw new InvalidXRoadQueryException($"Element `{reader.LocalName}` was unexpected at given location while deserializing type `{Definition.Name}`.");

            throw XRoadException.UnexpectedElementInQuery(Definition.Name, propertyMap.Definition.Content.SerializedName.LocalName, reader.LocalName);
        }

        private void ValidateRemainingProperties(IEnumerator<IPropertyMap> properties, ContentDefinition content)
        {
            while (properties.MoveNext())
            {
                var propertyMap = properties.Current ?? throw new NullReferenceException();
                if (propertyMap.Definition.Content.IsOptional)
                    continue;

                var typeName = Definition?.Name ?? ((content.Particle as ArrayItemDefinition)?.Array as PropertyDefinition)?.DeclaringTypeDefinition?.Name;

                throw new InvalidXRoadQueryException($"Element `{propertyMap.Definition.Content.SerializedName.LocalName}` is required by type `{typeName}` definition.");
            }
        }
    }
}