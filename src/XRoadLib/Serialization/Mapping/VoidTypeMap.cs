using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class VoidTypeMap : ITypeMap
    {
        public uint DtoVersion { get; set; }
        public Type RuntimeType => typeof(void);
        public bool IsSimpleType => true;
        public bool IsAnonymous { get; set; }

        public object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            throw new NotImplementedException();
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            throw new NotImplementedException();
        }

        public void InitializeProperties(IDictionary<Type, ITypeMap> partialTypeMaps, TypeDefinition typeDefinition)
        {
            throw new NotImplementedException();
        }
    }
}