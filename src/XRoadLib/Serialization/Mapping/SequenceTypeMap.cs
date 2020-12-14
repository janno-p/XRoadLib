using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class SequenceTypeMap<T> : CompositeTypeMap<T> where T : class, new()
    {
        public SequenceTypeMap(ISerializer serializer, TypeDefinition typeDefinition)
            : base(serializer, typeDefinition)
        { }

        public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var entity = new T();

            var specifiedMembers = templateNode.ChildNames.ToDictionary(x => x, _ => false);
            void OnMemberSpecified(string memberName) => specifiedMembers[memberName] = true;

            var validateRequired = content.Particle is RequestDefinition;

            if (ContentPropertyMap != null)
            {
                await ReadPropertyValueAsync(reader, ContentPropertyMap, templateNode[ContentPropertyMap.Definition.TemplateName, message.Version], message, validateRequired, entity, OnMemberSpecified).ConfigureAwait(false);
                return entity.SetSpecifiedMembers(specifiedMembers);
            }

            var properties = PropertyMaps.GetEnumerator();

            if (reader.IsEmptyElement)
            {
                ValidateRemainingProperties(properties, content);
                await reader.MoveNextAndReturnAsync(entity).ConfigureAwait(false);
                return entity.SetSpecifiedMembers(specifiedMembers);
            }

            var parentDepth = reader.Depth;
            var itemDepth = parentDepth + 1;

            await reader.ReadAsync().ConfigureAwait(false);

            while (parentDepth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != itemDepth)
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                    continue;
                }

                var propertyNode = MoveToProperty(reader, properties, templateNode, message, validateRequired);

                await ReadPropertyValueAsync(reader, properties.Current, propertyNode, message, validateRequired, entity, OnMemberSpecified).ConfigureAwait(false);
            }

            ValidateRemainingProperties(properties, content);

            return entity.SetSpecifiedMembers(specifiedMembers);
        }

        private async Task ReadPropertyValueAsync(XmlReader reader, IPropertyMap propertyMap, IXmlTemplateNode propertyNode, XRoadMessage message, bool validateRequired, T dtoObject, Action<string> onMemberSpecified)
        {
            if (propertyNode == null)
            {
                await reader.ConsumeUnusedElementAsync().ConfigureAwait(false);
                return;
            }

            var isNull = reader.IsNilElement();
            if (validateRequired && isNull && propertyNode.IsRequired)
                throw new ParameterRequiredException(Definition, propertyMap.Definition);

            var templateName = propertyMap.Definition.TemplateName;
            if ((isNull || await propertyMap.DeserializeAsync(reader, dtoObject, propertyNode, message).ConfigureAwait(false)) && !string.IsNullOrWhiteSpace(templateName))
                onMemberSpecified(templateName);

            await reader.ConsumeNilElementAsync(isNull).ConfigureAwait(false);
        }

        private IXmlTemplateNode MoveToProperty(XmlReader reader, IEnumerator<IPropertyMap> properties, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            IPropertyMap propertyMap = null;

            while (properties.MoveNext())
            {
                propertyMap = properties.Current ?? throw new NullReferenceException();

                var propertyName = propertyMap.Definition.Content.SerializedName;
                var propertyNode = templateNode[properties.Current.Definition.TemplateName, message.Version];

                if (reader.GetXName() == propertyName)
                    return propertyNode;

                if (!propertyMap.Definition.Content.IsOptional)
                    throw new UnexpectedElementException(Definition, propertyMap.Definition, reader.GetXName());

                if (validateRequired && propertyNode != null && propertyNode.IsRequired)
                    throw new ParameterRequiredException(Definition, propertyMap.Definition);
            }

            if (propertyMap == null)
                throw new InvalidQueryException($"Element `{reader.GetXName()}` was unexpected at given location while deserializing type `{Definition.Name}`.");

            throw new UnexpectedElementException(Definition, propertyMap.Definition, reader.GetXName());
        }

        private void ValidateRemainingProperties(IEnumerator<IPropertyMap> properties, ContentDefinition content)
        {
            while (properties.MoveNext())
            {
                var propertyMap = properties.Current ?? throw new NullReferenceException();
                if (propertyMap.Definition.Content.IsOptional)
                    continue;

                var typeName = Definition?.Name ?? ((content.Particle as ArrayItemDefinition)?.Array as PropertyDefinition)?.DeclaringTypeDefinition?.Name;

                throw new InvalidQueryException($"Element `{propertyMap.Definition.Content.SerializedName.LocalName}` is required by type `{typeName}` definition.");
            }
        }
    }
}