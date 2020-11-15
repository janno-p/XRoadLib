using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public abstract class CompositeTypeMap<T> : TypeMap, ICompositeTypeMap where T : class, IXRoadSerializable, new()
    {
        protected readonly ISerializer Serializer;
        protected readonly IList<IPropertyMap> PropertyMaps = new List<IPropertyMap>();

        protected IPropertyMap ContentPropertyMap;

        protected CompositeTypeMap(ISerializer serializer, TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            Serializer = serializer;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            message.Style.WriteType(writer, Definition, content);

            if (ContentPropertyMap != null)
            {
                var childTemplateNode = templateNode?[ContentPropertyMap.Definition.TemplateName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    ContentPropertyMap.Serialize(writer, childTemplateNode, value, message);
                return;
            }

            foreach (var propertyMap in PropertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.Definition.TemplateName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, message);
            }
        }

        public void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions, IEnumerable<string> availableFilters)
        {
            if (PropertyMaps.Count > 0)
                return;

            var createdPropertyMaps = propertyDefinitions.Select(x => new PropertyMap(Serializer, x.Item1, x.Item2, availableFilters))
                                                         .ToList();

            if (createdPropertyMaps.Count == 1 && createdPropertyMaps[0].Definition.Content.MergeContent && createdPropertyMaps[0].Definition.Content is SingularContentDefinition)
            {
                ContentPropertyMap = createdPropertyMaps[0];
                return;
            }

            foreach (var propertyMap in createdPropertyMaps)
            {
                if (propertyMap.Definition.Content.MergeContent && propertyMap.Definition.Content is SingularContentDefinition)
                    throw new SchemaDefinitionException($"Property {propertyMap.Definition} of type {Definition} cannot be merged, because mixed element content is not allowed.");

                AddPropertyMap(propertyMap);
            }
        }

        protected virtual void AddPropertyMap(IPropertyMap propertyMap)
        {
            PropertyMaps.Add(propertyMap);
        }
    }
}