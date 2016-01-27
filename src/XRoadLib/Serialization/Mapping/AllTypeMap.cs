using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AllTypeMap<T> : TypeMap<T> where T : class, IXRoadSerializable, new()
    {
        private readonly XName typeName;
        private readonly ISerializerCache serializerCache;
        private readonly IDictionary<string, IPropertyMap> deserializationPropertyMaps = new Dictionary<string, IPropertyMap>();
        private readonly IList<IPropertyMap> serializationPropertyMaps = new List<IPropertyMap>();

        public override bool IsSimpleType => false;

        public AllTypeMap(ISerializerCache serializerCache, XName typeName)
        {
            this.serializerCache = serializerCache;
            this.typeName = typeName;
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

            throw XRoadException.UnknownProperty(reader.LocalName, typeName);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            if (!IsAnonymous && (context.Protocol == XRoadProtocol.Version20 || runtimeType != fieldType))
                writer.WriteTypeAttribute(serializerCache.GetXmlTypeName(value.GetType()), templateNode?.Namespace);

            foreach (var propertyMap in serializationPropertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.PropertyName, DtoVersion];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, context);
            }
        }

        public override void InitializeProperties(IDictionary<Type, ITypeMap> partialTypeMaps, TypeDefinition typeDefinition)
        {
            if (deserializationPropertyMaps.Count > 0)
                return;

            var comparer = typeConfiguration?.GetPropertyComparer(runtimeType) ?? DefaultComparer.Instance;

            foreach (var propertyInfo in RuntimeType.GetAllPropertiesSorted(comparer, DtoVersion))
            {
                var qualifiedTypeName = propertyInfo.GetQualifiedElementDataType();

                var typeMap = qualifiedTypeName != null ? serializerCache.GetTypeMap(qualifiedTypeName, propertyInfo.PropertyType.IsArray, DtoVersion)
                                                        : serializerCache.GetTypeMap(propertyInfo.PropertyType, DtoVersion, partialTypeMaps);

                var propertyName = propertyInfo.GetPropertyName(typeConfiguration);

                var propertyMap = new PropertyMap(serializerCache, propertyName, propertyInfo, typeMap, RuntimeType);

                deserializationPropertyMaps.Add(propertyMap.PropertyName, propertyMap);
                serializationPropertyMaps.Add(propertyMap);
            }
        }
    }
}