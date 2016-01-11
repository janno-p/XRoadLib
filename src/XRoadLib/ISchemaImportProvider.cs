namespace XRoadLib
{
    public interface ISchemaImportProvider
    {
        string GetSchemaLocation(XRoadProtocol protocol);

        string GetSchemaNamespace(XRoadProtocol protocol);
    }
}