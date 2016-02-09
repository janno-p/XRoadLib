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

        private IPropertyMap mergePropertyMap;

        public AllTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            this.serializerCache = serializerCache;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            var dtoObject = new T();
            dtoObject.SetTemplateMembers(templateNode.ChildNames);

            if (reader.IsEmptyElement)
                return dtoObject;

            if (mergePropertyMap != null)
            {
                ReadPropertyValue(reader, mergePropertyMap, templateNode, message, validateRequired, dtoObject);
                return dtoObject;
            }

            var depth = reader.Depth;
            var requiredCount = 0;

            while (reader.Read() && depth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                var propertyMap = GetPropertyMap(reader);

                if (ReadPropertyValue(reader, propertyMap, templateNode, message, validateRequired, dtoObject) && validateRequired)
                    requiredCount++;
            }

            if (validateRequired && requiredCount < templateNode.CountRequiredNodes(message.Version))
                throw XRoadException.MissingRequiredPropertyValues(GetMissingRequiredPropertyNames(dtoObject, templateNode, message));

            return dtoObject;
        }

        private static bool ReadPropertyValue(XmlReader reader, IPropertyMap propertyMap, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired, T dtoObject)
        {
            var propertyNode = templateNode[propertyMap.Definition.TemplateName, message.Version];
            if (propertyNode == null)
            {
                reader.ReadToEndElement();
                return false;
            }

            var isNull = reader.IsNilElement();
            if (validateRequired && isNull && propertyNode.IsRequired)
                throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyMap.Definition.Name.LocalName, 1));

            if (isNull || propertyMap.Deserialize(reader, dtoObject, propertyNode, message))
                dtoObject.OnMemberDeserialized(reader.LocalName);

            return propertyNode.IsRequired;
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
                var childTemplateNode = templateNode?[propertyMap.Definition.TemplateName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, message);
            }
        }

        public override void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions, IEnumerable<string> availableFilters)
        {
            if (deserializationPropertyMaps.Count > 0)
                return;

            var createdPropertyMaps = propertyDefinitions.Select(x => new PropertyMap(serializerCache, x.Item1, x.Item2, availableFilters)).ToList();
            if (createdPropertyMaps.Count == 1 && createdPropertyMaps[0].Definition.MergeContent)
            {
                mergePropertyMap = createdPropertyMaps[0];
                return;
            }

            foreach (var propertyMap in createdPropertyMaps)
            {
                if (propertyMap.Definition.MergeContent)
                    throw new Exception($"Property {propertyMap.Definition} of type {Definition} cannot be merged, because there are more than 1 properties present.");

                deserializationPropertyMaps.Add(propertyMap.Definition.Name.LocalName, propertyMap);
                serializationPropertyMaps.Add(propertyMap);
            }
        }
    }
}