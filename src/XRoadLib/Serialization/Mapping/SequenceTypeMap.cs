using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class SequenceTypeMap<T> : TypeMap<T> where T : class, IXRoadSerializable, new()
    {
        private readonly ISerializerCache serializerCache;
        private readonly IList<IPropertyMap> propertyMaps = new List<IPropertyMap>();

        public override bool IsSimpleType => false;

        public SequenceTypeMap(ISerializerCache serializerCache)
        {
            this.serializerCache = serializerCache;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
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

                var childValidatorNode = templateNode[properties.Current.PropertyName, DtoVersion];
                if (childValidatorNode == null)
                {
                    reader.ReadToEndElement();
                    continue;
                }

                if (reader.IsNilElement() || properties.Current.Deserialize(reader, entity, childValidatorNode, context))
                    entity.OnMemberDeserialized(properties.Current.PropertyName);
            }

            return entity;
        }

        private void MoveToProperty(XmlReader reader, IEnumerator<IPropertyMap> properties)
        {
            while (properties.MoveNext())
                if (reader.LocalName == properties.Current.PropertyName)
                    return;

            throw XRoadException.InvalidQuery("Andmetüübil `{0}` puudub element `{1}` või see on esitatud vales kohas.", RuntimeType.Name, reader.LocalName);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            if (context.Protocol == XRoadProtocol.Version20 || runtimeType != fieldType)
                writer.WriteTypeAttribute(serializerCache.GetXmlTypeName(value.GetType()), templateNode?.Namespace);

            foreach (var propertyMap in propertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.PropertyName, DtoVersion];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, context);
            }
        }

        public override void InitializeProperties(IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps, XRoadProtocol protocol)
        {
            if (propertyMaps.Count > 0)
                return;

            var comparer = runtimeType.GetComparer(protocol);

            foreach (var propertyInfo in RuntimeType.GetAllPropertiesSorted(comparer, DtoVersion))
            {
                var dataType = propertyInfo.GetElementType();
                var elementTypeName = string.IsNullOrWhiteSpace(dataType) ? null : new XmlQualifiedName(dataType, dataType == "base64" || dataType == "hexBinary" ? NamespaceHelper.SOAP_ENC : NamespaceHelper.XSD);

                var typeMap = elementTypeName != null ? serializerCache.GetTypeMap(elementTypeName, DtoVersion, partialTypeMaps)
                                                      : serializerCache.GetTypeMap(propertyInfo.PropertyType, DtoVersion, partialTypeMaps);

                propertyMaps.Add(new PropertyMap(serializerCache, propertyInfo, typeMap, RuntimeType));
            }
        }
    }
}