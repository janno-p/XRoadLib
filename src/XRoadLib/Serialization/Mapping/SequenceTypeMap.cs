﻿using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping;

public class SequenceTypeMap<T> : CompositeTypeMap where T : class, IXRoadSerializable, new()
{
    public SequenceTypeMap(ISerializer serializer, TypeDefinition typeDefinition)
        : base(serializer, typeDefinition)
    { }

    public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
    {
        var entity = new T();
        entity.SetTemplateMembers(templateNode.ChildNames);

        var validateRequired = content.Particle is RequestDefinition;

        if (ContentPropertyMap != null)
        {
            await ReadPropertyValueAsync(reader, ContentPropertyMap, templateNode[ContentPropertyMap.Definition.TemplateName, message.Version], message, validateRequired, entity).ConfigureAwait(false);
            return entity;
        }

        var properties = PropertyMaps.GetEnumerator();

        if (reader.IsEmptyElement)
        {
            ValidateRemainingProperties(properties, content);
            return await reader.MoveNextAndReturnAsync(entity).ConfigureAwait(false);
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

            await ReadPropertyValueAsync(reader, properties.Current, propertyNode, message, validateRequired, entity).ConfigureAwait(false);
        }

        ValidateRemainingProperties(properties, content);

        return entity;
    }

    private async Task ReadPropertyValueAsync(XmlReader reader, IPropertyMap propertyMap, IXmlTemplateNode propertyNode, XRoadMessage message, bool validateRequired, T dtoObject)
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
            dtoObject.OnMemberDeserialized(templateName);

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

            if (validateRequired && propertyNode is { IsRequired: true })
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