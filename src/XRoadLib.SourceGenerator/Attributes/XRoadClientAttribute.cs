namespace XRoadLib.SourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class XRoadClientAttribute : Attribute
{
    public string WsdlFile { get; }

    public XRoadClientAttribute(string wsdlFile)
    {
        WsdlFile = wsdlFile;
    }
}
