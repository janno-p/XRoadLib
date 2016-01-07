using System.Xml;

namespace XRoadLib.Serialization
{
    public class CustomSerialization : ICustomSerialization
    {
        public virtual void OnContentComplete(XmlWriter writer)
        { }
    }
}