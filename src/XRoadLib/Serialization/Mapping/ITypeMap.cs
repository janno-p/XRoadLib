using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface ITypeMap
    {
        TypeDefinition TypeDefinition { get; }

        object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context);

        void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context);

        void InitializeProperties(IEnumerable<PropertyDefinition> propertyDefinitions);
    }
}