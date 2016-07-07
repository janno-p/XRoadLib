using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AllTypeMap<T> : CompositeTypeMap<T> where T : class, IXRoadSerializable, new()
    {
        private readonly IDictionary<string, IPropertyMap> deserializationPropertyMaps = new Dictionary<string, IPropertyMap>();
        private readonly Lazy<int> requiredPropertiesCount;

        public AllTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
            : base(serializerCache, typeDefinition)
        {
            requiredPropertiesCount = new Lazy<int>(() => deserializationPropertyMaps.Where(x => !x.Value.Definition.IsOptional).Count());
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            var dtoObject = new T();
            dtoObject.SetTemplateMembers(templateNode.ChildNames);

            var validateRequired = definition is RequestValueDefinition;

            if (contentPropertyMap != null)
            {
                ReadPropertyValue(reader, contentPropertyMap, templateNode, message, validateRequired, dtoObject);
                return dtoObject;
            }

            var existingPropertyNames = new HashSet<string>();

            if (reader.IsEmptyElement)
            {
                ValidateRemainingProperties(existingPropertyNames, definition);
                return MoveNextAndReturn(reader, dtoObject);
            }

            var parentDepth = reader.Depth;
            var itemDepth = parentDepth + 1;
            var requiredCount = 0;

            reader.Read();

            while (parentDepth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != itemDepth)
                {
                    reader.Read();
                    continue;
                }

                var propertyMap = GetPropertyMap(reader);

                if (!propertyMap.Definition.IsOptional)
                    existingPropertyNames.Add(reader.LocalName);

                if (ReadPropertyValue(reader, propertyMap, templateNode, message, validateRequired, dtoObject) && validateRequired)
                    requiredCount++;
            }

            if (validateRequired && requiredCount < templateNode.CountRequiredNodes(message.Version))
                throw XRoadException.MissingRequiredPropertyValues(GetMissingRequiredPropertyNames(dtoObject, templateNode, message));

            ValidateRemainingProperties(existingPropertyNames, definition);

            return dtoObject;
        }

        private static bool ReadPropertyValue(XmlReader reader, IPropertyMap propertyMap, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired, T dtoObject)
        {
            var templateName = propertyMap.Definition.TemplateName;

            var propertyNode = templateNode[templateName, message.Version];
            if (propertyNode == null)
            {
                reader.ConsumeUnusedElement();
                return false;
            }

            var isNull = reader.IsNilElement();
            if (validateRequired && isNull && propertyNode.IsRequired)
                throw XRoadException.MissingRequiredPropertyValues(Enumerable.Repeat(propertyMap.Definition.Name.LocalName, 1));

            if ((isNull || propertyMap.Deserialize(reader, dtoObject, propertyNode, message)) && !string.IsNullOrWhiteSpace(templateName))
                dtoObject.OnMemberDeserialized(templateName);

            reader.ConsumeNilElement(isNull);

            return propertyNode.IsRequired;
        }

        private static IEnumerable<string> GetMissingRequiredPropertyNames(IXRoadSerializable dtoObject, IXmlTemplateNode templateNode, XRoadMessage message)
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

        protected override void AddPropertyMap(IPropertyMap propertyMap)
        {
            base.AddPropertyMap(propertyMap);

            var propertyName = propertyMap.Definition.MergeContent ? propertyMap.Definition.ArrayItemDefinition.Name.LocalName : propertyMap.Definition.Name.LocalName;

            deserializationPropertyMaps.Add(propertyName, propertyMap);
        }

        private void ValidateRemainingProperties(ISet<string> existingPropertyNames, IContentDefinition contentDefinition)
        {
            if (existingPropertyNames.Count == requiredPropertiesCount.Value)
                return;

            var typeName = Definition?.Name ?? (((contentDefinition as ArrayItemDefinition)?.WrapperDefinition) as PropertyDefinition)?.DeclaringTypeDefinition?.Name;
            var missingProperties = deserializationPropertyMaps.Where(kv => !existingPropertyNames.Contains(kv.Key) && !kv.Value.Definition.IsOptional).Select(kv => $"`{kv.Key}`").ToList();

            var propertyMessage = string.Join(", ", missingProperties);
            var errorMessage = missingProperties.Count > 1
                ? $"Elements {propertyMessage} are required by type `{typeName}` definition."
                : $"Element {propertyMessage} is required by type `{typeName}` definition.";

            throw XRoadException.InvalidQuery(errorMessage);
        }
    }
}