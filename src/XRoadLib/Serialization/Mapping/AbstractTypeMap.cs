using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AbstractTypeMap<T> : TypeMap<T>, IAbstractTypeMap
    {
        private readonly TypeDefinition typeDefinition;

        public override bool IsSimpleType => false;
        public override XName QualifiedName => typeDefinition.Name;

        public AbstractTypeMap(TypeDefinition typeDefinition)
        {
            this.typeDefinition = typeDefinition;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            throw XRoadException.TypeAttributeRequired(RuntimeType.Name);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            throw new NotImplementedException();
        }
    }
}