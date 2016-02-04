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

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            var dtoObject = new T();
            dtoObject.SetTemplateMembers(templateNode.ChildNames);

            if (reader.IsEmptyElement)
                return dtoObject;

            var depth = reader.Depth;
            var requiredCount = 0;

            while (reader.Read() && depth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                var propertyMap = GetPropertyMap(reader);

                var propertyNode = templateNode[reader.LocalName, message.Version];
                if (propertyNode == null)
                {
                    reader.ReadToEndElement();
                    continue;
                }

                var isNull = reader.IsNilElement();
                if (isNull && propertyNode.IsRequired)
                    throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyMap.Definition.Name.LocalName, 1));

                if (propertyNode.IsRequired)
                    requiredCount++;

                if (isNull || propertyMap.Deserialize(reader, dtoObject, propertyNode, message))
                    dtoObject.OnMemberDeserialized(reader.LocalName);
            }

            if (requiredCount < templateNode.CountRequiredNodes(message.Version))
                throw XRoadException.MissingRequiredPropertyValues(GetMissingRequiredPropertyNames(dtoObject, templateNode, message));

            return dtoObject;
        }

        private IEnumerable<string> GetMissingRequiredPropertyNames(IXRoadSerializable dtoObject, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            return templateNode.ChildNames
                               .Select(n => templateNode[n, message.Version])
                               .Where(n => n.IsRequired)
                               .Where(n => !dtoObject.IsSpecified(n.Name))
                               .Select(n => n.Name);
        }

        private IPropertyMap GetPropertyMap(XmlReader reader)
        {
            IPropertyMap propertyMap;
            if (deserializationPropertyMaps.TryGetValue(reader.LocalName, out propertyMap))
                return propertyMap;

            throw XRoadException.UnknownProperty(reader.LocalName, Definition.Name);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            message.Protocol.Style.WriteType(writer, Definition, expectedType);

            foreach (var propertyMap in serializationPropertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.Definition.Name.LocalName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, message);
            }
        }

        public override void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions)
        {
            if (deserializationPropertyMaps.Count > 0)
                return;

            foreach (var propertyMap in propertyDefinitions.Select(x => new PropertyMap(serializerCache, x.Item1, x.Item2)))
            {
                deserializationPropertyMaps.Add(propertyMap.Definition.Name.LocalName, propertyMap);
                serializationPropertyMaps.Add(propertyMap);
            }
        }
    }
}