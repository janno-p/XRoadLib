using System.Xml;

namespace XRoadLib.Serialization
{
    public interface IXRoadXmlSerializable
    {
        void ReadXml(XmlReader reader, XRoadMessage message);
        void WriteXml(XmlWriter writer, XRoadMessage message);
    }
}