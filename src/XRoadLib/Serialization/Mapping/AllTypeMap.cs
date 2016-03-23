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

        public AllTypeMap(ISerializerCache serializerCache, TypeDefinition typeDefinition)
            : base(serializerCache, typeDefinition)
        { }

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

            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, dtoObject);

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

                if (ReadPropertyValue(reader, propertyMap, templateNode, message, validateRequired, dtoObject) && validateRequired)
                    requiredCount++;
            }

            if (validateRequired && requiredCount < templateNode.CountRequiredNodes(message.Version))
                throw XRoadException.MissingRequiredPropertyValues(GetMissingRequiredPropertyNames(dtoObject, templateNode, message));

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
    }
}