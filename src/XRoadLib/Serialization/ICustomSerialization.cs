using System.Xml;

namespace XRoadLib.Serialization
{
    public interface ICustomSerialization
    {
        void OnContentComplete(XmlWriter writer);
    }
}