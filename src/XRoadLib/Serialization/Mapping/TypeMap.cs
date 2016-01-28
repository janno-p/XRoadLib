using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public abstract class TypeMap<T> : ITypeMap
    {
        protected readonly T defaultValue = default(T);
        protected readonly Type runtimeType = typeof(T);

        public uint DtoVersion { get; set; }
        public Type RuntimeType => runtimeType;
        public virtual bool IsSimpleType => true;
        public virtual bool IsAnonymous => false;

        public abstract object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context);

        public abstract void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context);

        public virtual void InitializeProperties(IDictionary<Type, ITypeMap> partialTypeMaps, IEnumerable<PropertyDefinition> propertyDefinitions) { }
    }
}