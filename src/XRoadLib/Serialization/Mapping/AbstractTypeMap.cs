using System;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AbstractTypeMap<T> : TypeMap<T>, IAbstractTypeMap
    {
        public AbstractTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            throw XRoadException.TypeAttributeRequired(Definition.Name.ToString());
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            throw new NotImplementedException();
        }
    }
}