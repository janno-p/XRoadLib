using System;
using System.Collections.Generic;
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
        private readonly TypeDefinition typeDefinition;

        public override bool IsSimpleType => false;

        public AllTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
        {
            this.serializerCache = serializerCache;
            this.typeDefinition = typeDefinition;
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

                var childValidatorNode = templateNode[reader.LocalName, DtoVersion];
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

            throw XRoadException.UnknownProperty(reader.LocalName, typeDefinition.Name);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            context.Protocol.Style.WriteType(writer, typeDefinition, expectedType);

            foreach (var propertyMap in serializationPropertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.PropertyName, DtoVersion];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, context);
            }
        }

        public override void InitializeProperties(IDictionary<Type, ITypeMap> partialTypeMaps, IEnumerable<PropertyDefinition> propertyDefinitions)
        {
            if (deserializationPropertyMaps.Count > 0)
                return;

            foreach (var propertyDefinition in propertyDefinitions)
            {
                var typeMap = propertyDefinition.TypeName != null
                    ? serializerCache.GetTypeMap(propertyDefinition.TypeName, propertyDefinition.ItemProperty != null, DtoVersion)
                    : serializerCache.GetTypeMap(propertyDefinition.RuntimeInfo.PropertyType, DtoVersion, partialTypeMaps);

                var propertyMap = new PropertyMap(serializerCache, propertyDefinition.Name.LocalName, propertyDefinition.RuntimeInfo, typeMap, RuntimeType);

                deserializationPropertyMaps.Add(propertyMap.PropertyName, propertyMap);
                serializationPropertyMaps.Add(propertyMap);
            }
        }
    }
}