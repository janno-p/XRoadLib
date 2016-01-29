using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AllTypeMap<T> : TypeMap<T> where T : class, IXRoadSerializable, new()
    {
        private readonly ISerializerCache serializerCache;
        private readonly IDictionary<string, IPropertyMap> deserializationPropertyMaps = new Dictionary<string, IPropertyMap>();
        private readonly IList<IPropertyMap> serializationPropertyMaps = new List<IPropertyMap>();

        public AllTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            this.serializerCache = serializerCache;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            var dtoObject = new T();
            dtoObject.SetTemplateMembers(templateNode.ChildNames);

            if (reader.IsEmptyElement)
                return dtoObject;

            var depth = reader.Depth;
            while (reader.Read() && depth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                var propertyMap = GetPropertyMap(reader);

                var childValidatorNode = templateNode[reader.LocalName, context.DtoVersion];
                if (childValidatorNode == null)
                {
                    reader.ReadToEndElement();
                    continue;
                }

                if (reader.IsNilElement() || propertyMap.Deserialize(reader, dtoObject, childValidatorNode, context))
                    dtoObject.OnMemberDeserialized(reader.LocalName);
            }

            return dtoObject;
        }

        private IPropertyMap GetPropertyMap(XmlReader reader)
        {
            IPropertyMap propertyMap;
            if (deserializationPropertyMaps.TryGetValue(reader.LocalName, out propertyMap))
                return propertyMap;

            throw XRoadException.UnknownProperty(reader.LocalName, TypeDefinition.Name);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            context.Protocol.Style.WriteType(writer, TypeDefinition, expectedType);

            foreach (var propertyMap in serializationPropertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.PropertyName, context.DtoVersion];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, context);
            }
        }

        public override void InitializeProperties(IEnumerable<PropertyDefinition> propertyDefinitions)
        {
            if (deserializationPropertyMaps.Count > 0)
                return;

            foreach (var propertyMap in propertyDefinitions.Select(propertyDefinition => new PropertyMap(serializerCache, propertyDefinition)))
            {
                deserializationPropertyMaps.Add(propertyMap.PropertyName, propertyMap);
                serializationPropertyMaps.Add(propertyMap);
            }
        }
    }
}