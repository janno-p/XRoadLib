using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class VoidTypeMap : ITypeMap
    {
        public Type RuntimeType => typeof(void);
        public bool IsSimpleType => true;
        public bool IsAnonymous => false;

        public object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            throw new NotImplementedException();
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            throw new NotImplementedException();
        }

        public void InitializeProperties(IEnumerable<PropertyDefinition> propertyDefinitions)
        {
            throw new NotImplementedException();
        }
    }
}