using System.Xml;

namespace XRoadLib.Wsdl
{
    public abstract class ServiceDescriptionFormatExtension
    {
        internal abstract void Write(XmlWriter writer);
    }
}