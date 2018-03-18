using System;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class AbstractTypeMap : TypeMap, IAbstractTypeMap
    {
        public AbstractTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            throw new InvalidQueryException($"The type '{Definition.Name}' is abstract, type attribute is required to specify target type.");
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            throw new NotImplementedException();
        }
    }
}