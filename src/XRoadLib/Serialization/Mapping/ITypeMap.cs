using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface ITypeMap
    {
        uint DtoVersion { get; set; }

        Type RuntimeType { get; }

        bool IsSimpleType { get; }

        object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context);

        void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context);

        void InitializeProperties(IDictionary<Type, ITypeMap> partialTypeMaps, IEnumerable<PropertyDefinition> propertyDefinitions);
    }
}