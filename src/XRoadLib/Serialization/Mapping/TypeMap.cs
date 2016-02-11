using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public abstract class TypeMap : ITypeMap
    {
        public TypeDefinition Definition { get; }

        protected TypeMap(TypeDefinition typeDefinition)
        {
            Definition = typeDefinition;
        }

        public abstract object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message);

        public abstract void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message);

        protected static object MoveNextAndReturn(XmlReader reader, object value)
        {
            reader.Read();
            return value;
        }
    }
}