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

        public TypeDefinition TypeDefinition { get; }

        protected TypeMap(TypeDefinition typeDefinition)
        {
            TypeDefinition = typeDefinition;
            TypeDefinition.TypeMap = this;
        }

        public abstract object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context);

        public abstract void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context);

        public virtual void InitializeProperties(IEnumerable<PropertyDefinition> propertyDefinitions)
        { }
    }
}