namespace XRoadLib.Configuration
{
    public interface ISchemaImportProvider
    {
        string SchemaLocation { get; }

        string SchemaNamespace { get; }
    }
}