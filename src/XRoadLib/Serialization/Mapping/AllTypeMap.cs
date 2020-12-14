using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AllTypeMap<T> : CompositeTypeMap<T> where T : class, new()
    {
        private readonly IDictionary<XName, IPropertyMap> _deserializationPropertyMaps = new Dictionary<XName, IPropertyMap>();
        private readonly Lazy<int> _requiredPropertiesCount;

        public AllTypeMap(ISerializer serializer, TypeDefinition typeDefinition)
            : base(serializer, typeDefinition)
        {
            _requiredPropertiesCount = new Lazy<int>(() => _deserializationPropertyMaps.Count(x => !x.Value.Definition.Content.IsOptional));
        }

        public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var dtoObject = new T();

            var specifiedMembers = templateNode.ChildNames.ToDictionary(x => x, _ => false);
            void OnMemberSpecified(string memberName) => specifiedMembers[memberName] = true;

            var validateRequired = content.Particle is RequestDefinition;

            if (ContentPropertyMap != null)
            {
                await ReadPropertyValueAsync(reader, ContentPropertyMap, templateNode, message, validateRequired, dtoObject, OnMemberSpecified).ConfigureAwait(false);
                return dtoObject.SetSpecifiedMembers(specifiedMembers);
            }

            var existingPropertyNames = new HashSet<XName>();

            if (reader.IsEmptyElement)
            {
                ValidateRemainingProperties(existingPropertyNames, content);
                await reader.MoveNextAndReturnAsync(dtoObject).ConfigureAwait(false);
                return dtoObject.SetSpecifiedMembers(specifiedMembers);
            }

            var parentDepth = reader.Depth;
            var itemDepth = parentDepth + 1;
            var requiredCount = 0;

            await reader.ReadAsync().ConfigureAwait(false);

            while (parentDepth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != itemDepth)
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                    continue;
                }

                var propertyMap = GetPropertyMap(reader);

                if (!propertyMap.Definition.Content.IsOptional)
                    existingPropertyNames.Add(reader.GetXName());

                if (await ReadPropertyValueAsync(reader, propertyMap, templateNode, message, validateRequired, dtoObject, OnMemberSpecified).ConfigureAwait(false) && validateRequired)
                    requiredCount++;
            }

            if (validateRequired && requiredCount < templateNode.CountRequiredNodes(message.Version))
            {
                var properties = GetMissingRequiredProperties(specifiedMembers, templateNode, message).ToList();
                throw new ParameterRequiredException(Definition, properties);
            }

            ValidateRemainingProperties(existingPropertyNames, content);

            return dtoObject.SetSpecifiedMembers(specifiedMembers);
        }

        private async Task<bool> ReadPropertyValueAsync(XmlReader reader, IPropertyMap propertyMap, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired, T dtoObject, Action<string> onMemberSpecified)
        {
            var templateName = propertyMap.Definition.TemplateName;

            var propertyNode = templateNode[templateName, message.Version];
            if (propertyNode == null)
            {
                await reader.ConsumeUnusedElementAsync().ConfigureAwait(false);
                return false;
            }

            var isNull = reader.IsNilElement();
            if (validateRequired && isNull && propertyNode.IsRequired)
                throw new ParameterRequiredException(Definition, propertyMap.Definition);

            if ((isNull || await propertyMap.DeserializeAsync(reader, dtoObject, propertyNode, message).ConfigureAwait(false)) && !string.IsNullOrWhiteSpace(templateName))
                onMemberSpecified(templateName);

            await reader.ConsumeNilElementAsync(isNull).ConfigureAwait(false);

            return propertyNode.IsRequired;
        }

        private IEnumerable<PropertyDefinition> GetMissingRequiredProperties(IDictionary<string, bool> specifiedMembers, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            return templateNode.ChildNames
                               .Select(n => templateNode[n, message.Version])
                               .Where(n => n.IsRequired)
                               .Where(n => !(specifiedMembers.TryGetValue(n.Name, out var value) && value))
                               .Select(n => _deserializationPropertyMaps[n.Name].Definition);
        }

        private IPropertyMap GetPropertyMap(XmlReader reader)
        {
            var readerName = reader.GetXName();

            return _deserializationPropertyMaps.TryGetValue(readerName, out var propertyMap)
                ? propertyMap
                : throw new UnknownPropertyException($"Type `{Definition.Name}` does not define property `{readerName}` (property names are case-sensitive).", Definition, readerName);
        }

        protected override void AddPropertyMap(IPropertyMap propertyMap)
        {
            base.AddPropertyMap(propertyMap);

            _deserializationPropertyMaps.Add(propertyMap.Definition.Content.SerializedName.LocalName, propertyMap);
        }

        private void ValidateRemainingProperties(ICollection<XName> existingPropertyNames, ContentDefinition content)
        {
            if (existingPropertyNames.Count == _requiredPropertiesCount.Value)
                return;

            var typeName = Definition?.Name ?? ((content.Particle as ArrayItemDefinition)?.Array as PropertyDefinition)?.DeclaringTypeDefinition?.Name;
            var missingProperties = _deserializationPropertyMaps.Where(kv => !existingPropertyNames.Contains(kv.Key) && !kv.Value.Definition.Content.IsOptional).Select(kv => $"`{kv.Key}`").ToList();

            var propertyMessage = string.Join(", ", missingProperties);
            if (missingProperties.Count > 0)
                throw new InvalidQueryException($"Elements {propertyMessage} are required by type `{typeName}` definition.");

            throw new InvalidQueryException($"Element {propertyMessage} is required by type `{typeName}` definition.");
        }
    }
}