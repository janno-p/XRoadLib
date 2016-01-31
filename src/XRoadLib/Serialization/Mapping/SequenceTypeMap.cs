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

        public SequenceTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            this.serializerCache = serializerCache;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            var entity = new T();
            entity.SetTemplateMembers(templateNode.ChildNames);

            if (reader.IsEmptyElement)
                return entity;

            var depth = reader.Depth;
            var properties = propertyMaps.GetEnumerator();

            while (reader.Read() && depth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                MoveToProperty(reader, properties);

                var childValidatorNode = templateNode[properties.Current.Definition.Name.LocalName, message.Version];
                if (childValidatorNode == null)
                {
                    reader.ReadToEndElement();
                    continue;
                }

                if (reader.IsNilElement() || properties.Current.Deserialize(reader, entity, childValidatorNode, message))
                    entity.OnMemberDeserialized(properties.Current.Definition.Name.LocalName);
            }

            return entity;
        }

        private void MoveToProperty(XmlReader reader, IEnumerator<IPropertyMap> properties)
        {
            while (properties.MoveNext())
                if (reader.LocalName == properties.Current.Definition.Name.LocalName)
                    return;

            throw XRoadException.InvalidQuery("Andmetüübil `{0}` puudub element `{1}` või see on esitatud vales kohas.", Definition.Name, reader.LocalName);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            message.Protocol.Style.WriteType(writer, Definition, expectedType);

            foreach (var propertyMap in propertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.Definition.Name.LocalName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, message);
            }
        }

        public override void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions)
        {
            if (propertyMaps.Count > 0)
                return;

            foreach (var propertyMap in propertyDefinitions.Select(x => new PropertyMap(serializerCache, x.Item1, x.Item2)))
                propertyMaps.Add(propertyMap);
        }
    }
}