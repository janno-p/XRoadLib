using XRoadLib.Wsdl;

namespace XRoadLib.Schema;

/// <summary>
/// Basic implementation for <see cref="ISchemaExporter"/>.
/// Base type for protocol specific implementations of schema.
/// </summary>
public abstract class AbstractSchemaExporter : ISchemaExporter
{
    /// <summary>
    /// Producer namespace of exported X-Road schema.
    /// </summary>
    [UsedImplicitly]
    protected virtual string ProducerNamespace { get; }

    /// <inheritdoc />
    public abstract string XRoadPrefix { get; }

    /// <inheritdoc />
    public abstract string XRoadNamespace { get; }

    /// <summary>
    /// Initializes abstract schema exporter with producer namespace.
    /// </summary>
    protected AbstractSchemaExporter(string producerNamespace)
    {
        ProducerNamespace = producerNamespace;
    }

    /// <inheritdoc />
    public virtual void ExportOperationDefinition(OperationDefinition operationDefinition) { }

    /// <inheritdoc />
    public virtual void ExportPropertyDefinition(PropertyDefinition propertyDefinition) { }

    /// <inheritdoc />
    public virtual void ExportTypeDefinition(TypeDefinition typeDefinition) { }

    /// <inheritdoc />
    public virtual void ExportResponseDefinition(ResponseDefinition responseDefinition) { }

    /// <inheritdoc />
    public virtual void ExportRequestDefinition(RequestDefinition requestDefinition) { }

    /// <inheritdoc />
    public virtual void ExportFaultDefinition(FaultDefinition faultDefinition) { }

    /// <inheritdoc />
    public virtual string ExportSchemaLocation(string namespaceName) => null;

    /// <inheritdoc />
    public virtual void ExportServiceDescription(ServiceDescription serviceDescription) { }

    /// <inheritdoc />
    public virtual void ExportHeaderDefinition(HeaderDefinition headerDefinition) { }

    /// <inheritdoc />
    public virtual void ExportProtocolDefinition(ProtocolDefinition protocolDefinition)
    {
        protocolDefinition.ProducerNamespace = ProducerNamespace;
    }

    /// <inheritdoc />
    public virtual bool IsQualifiedElementDefault(string namespaceName) => false;
}