using System.Xml;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface IPropertyMap
    {
        string PropertyName { get; }

        bool Deserialize(XmlReader reader, IXRoadSerializable dtoObject, IXmlTemplateNode templateNode, XRoadMessage message);

        void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, XRoadMessage message);
    }
}