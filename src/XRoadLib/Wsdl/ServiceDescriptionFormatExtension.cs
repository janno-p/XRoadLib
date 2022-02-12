namespace XRoadLib.Wsdl;

public abstract class ServiceDescriptionFormatExtension
{
    internal abstract Task WriteAsync(XmlWriter writer);
}