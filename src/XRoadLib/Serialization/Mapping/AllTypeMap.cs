using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AllTypeMap<T> : TypeMap<T> where T : class, IXRoadSerializable, new()
    {
        private readonly ISerializerCache serializerCache;
        private readonly IDictionary<string, IPropertyMap> deserializationPropertyMaps = new Dictionary<string, IPropertyMap>();
        private readonly IList<IPropertyMap> serializationPropertyMaps = new List<IPropertyMap>();

        public override bool IsSimpleType => false;

        public AllTypeMap(ISerializerCache serializerCache)
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

            throw XRoadException.UnknownProperty(runtimeType, reader.LocalName);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            if (context.Protocol == XRoadProtocol.Version20 || runtimeType != fieldType)
                writer.WriteTypeAttribute(serializerCache.GetXmlTypeName(value.GetType()), templateNode?.Namespace);

            foreach (var propertyMap in serializationPropertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.PropertyName, DtoVersion];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, context);
            }
        }

        public override void InitializeProperties(IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps, XRoadProtocol protocol)
        {
            if (deserializationPropertyMaps.Count > 0)
                return;

            var comparer = runtimeType.GetComparer(protocol);

            foreach (var propertyInfo in RuntimeType.GetAllPropertiesSorted(comparer, DtoVersion))
            {
                var qualifiedTypeName = propertyInfo.GetQualifiedTypeName();

                var typeMap = qualifiedTypeName != null ? serializerCache.GetTypeMap(qualifiedTypeName, DtoVersion, partialTypeMaps)
                                                        : serializerCache.GetTypeMap(propertyInfo.PropertyType, DtoVersion, partialTypeMaps);

                var propertyMap = new PropertyMap(serializerCache, propertyInfo, typeMap, RuntimeType);

                deserializationPropertyMaps.Add(propertyMap.PropertyName, propertyMap);
                serializationPropertyMaps.Add(propertyMap);
            }
        }
    }
}