using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface ITypeMap
    {
        TypeDefinition Definition { get; }

        object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired = false);

        void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message);

        void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions, IEnumerable<string> availableFilters);
    }
}