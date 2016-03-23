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

            var validateRequired = definition is RequestValueDefinition;

            if (contentPropertyMap != null)
            {
                ReadPropertyValue(reader, contentPropertyMap, templateNode[contentPropertyMap.Definition.TemplateName, message.Version], message, validateRequired, entity);
                return entity;
            }

            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, entity);

            var parentDepth = reader.Depth;
            var itemDepth = parentDepth + 1;
            var properties = propertyMaps.GetEnumerator();

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
                throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyMap.Definition.Name.LocalName, 1));

            var templateName = propertyMap.Definition.TemplateName;
            if ((isNull || propertyMap.Deserialize(reader, dtoObject, propertyNode, message)) && !string.IsNullOrWhiteSpace(templateName))
                dtoObject.OnMemberDeserialized(templateName);

            reader.ConsumeNilElement(isNull);
        }

        private IXmlTemplateNode MoveToProperty(XmlReader reader, IEnumerator<IPropertyMap> properties, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            while (properties.MoveNext())
            {
                var propertyMap = properties.Current;
                var propertyName = propertyMap.Definition.MergeContent ? propertyMap.Definition.ArrayItemDefinition.Name.LocalName : propertyMap.Definition.Name.LocalName;
                var propertyNode = templateNode[properties.Current.Definition.TemplateName, message.Version];

                if (reader.LocalName == propertyName)
                    return propertyNode;

                if (validateRequired && propertyNode != null && propertyNode.IsRequired)
                    throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyName, 1));
            }

            throw XRoadException.InvalidQuery($"Andmetüübil `{Definition.Name}` puudub element `{reader.LocalName}` või see on esitatud vales kohas.");
        }
    }
}