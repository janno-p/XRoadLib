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
        protected readonly ISerializer serializer;
        protected readonly IList<IPropertyMap> propertyMaps = new List<IPropertyMap>();

        protected IPropertyMap contentPropertyMap;

        protected CompositeTypeMap(ISerializer serializer, TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            this.serializer = serializer;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            message.Style.WriteType(writer, Definition, content.RuntimeType, content.Particle is RequestDefinition);

            if (contentPropertyMap != null)
            {
                var childTemplateNode = templateNode?[contentPropertyMap.Definition.TemplateName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    contentPropertyMap.Serialize(writer, childTemplateNode, value, message);
                return;
            }

            foreach (var propertyMap in propertyMaps)
            {
                var childTemplateNode = templateNode?[propertyMap.Definition.TemplateName, message.Version];
                if (templateNode == null || childTemplateNode != null)
                    propertyMap.Serialize(writer, childTemplateNode, value, message);
            }
        }

        public void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions, IEnumerable<string> availableFilters)
        {
            if (propertyMaps.Count > 0)
                return;

            var createdPropertyMaps = propertyDefinitions.Select(x => new PropertyMap(serializer, x.Item1, x.Item2, availableFilters))
                                                         .ToList();

            if (createdPropertyMaps.Count == 1 && createdPropertyMaps[0].Definition.Content.MergeContent && createdPropertyMaps[0].Definition.Content is SingularContentDefinition)
            {
                contentPropertyMap = createdPropertyMaps[0];
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
            propertyMaps.Add(propertyMap);
        }
    }
}