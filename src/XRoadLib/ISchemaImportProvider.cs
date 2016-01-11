namespace XRoadLib
{
    public interface ISchemaImportProvider
    {
        string SchemaLocation { get; }
        string SchemaNamespace { get; }
    }
}