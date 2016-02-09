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

        public TypeDefinition Definition { get; }

        protected TypeMap(TypeDefinition typeDefinition)
        {
            Definition = typeDefinition;
        }

        public abstract object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired);

        public abstract void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message);

        public virtual void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions, IEnumerable<string> availableFilters)
        { }
    }
}