using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface ITypeMap
    {
        Type RuntimeType { get; }

        bool IsSimpleType { get; }

        bool IsAnonymous { get; }

        XName QualifiedName { get; }

        object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context);

        void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context);

        void InitializeProperties(IEnumerable<PropertyDefinition> propertyDefinitions);
    }
}