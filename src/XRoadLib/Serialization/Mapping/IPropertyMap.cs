using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface IPropertyMap
    {
        PropertyDefinition Definition { get; }

        Task<bool> DeserializeAsync(XmlReader reader, object dtoObject, IXmlTemplateNode templateNode, XRoadMessage message);

        Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, XRoadMessage message);
    }
}