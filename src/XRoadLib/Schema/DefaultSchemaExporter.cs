using System.Reflection;

namespace XRoadLib.Schema;

/// <summary>
/// Default schema exporter of X-Road message protocol version 4.0.
/// Generates schema description and serialization logic according to
/// default settings.
/// </summary>
public class DefaultSchemaExporter : AbstractSchemaExporter
{
    private readonly Assembly _contractAssembly;

    /// <summary>
    /// Preferred X-Road namespace prefix of the message protocol version.
    /// </summary>
    public override string XRoadPrefix => PrefixConstants.XRoad;

    /// <summary>
    /// X-Road specification namespace of the message protocol version.
    /// </summary>
    public override string XRoadNamespace => NamespaceConstants.XRoad;

    /// <summary>
    /// Defines list of supported DTO versions (for DTO based versioning).
    /// </summary>
    [UsedImplicitly]
    public IEnumerable<uint> SupportedVersions { get; }

    /// <summary>
    /// Define list of content filters of X-Road message elements.
    /// </summary>
    [UsedImplicitly]
    public IEnumerable<string> EnabledFilters { get; }

    public DefaultSchemaExporter(string producerNamespace, Assembly contractAssembly)
        : this(producerNamespace, contractAssembly, Enumerable.Empty<uint>(), Enumerable.Empty<string>())
    { }
        
    public DefaultSchemaExporter(string producerNamespace, Assembly contractAssembly, IEnumerable<uint> supportedVersions)
        : this(producerNamespace, contractAssembly, supportedVersions, Enumerable.Empty<string>())
    { }
        
    /// <summary>
    /// Initializes new schema exporter instance and configure minimal set
    /// of configuration options.
    /// </summary>
    [UsedImplicitly]
    public DefaultSchemaExporter(string producerNamespace, Assembly contractAssembly, IEnumerable<uint> supportedVersions, IEnumerable<string> enabledFilters)
        : base(producerNamespace)
    {
        _contractAssembly = contractAssembly;

        EnabledFilters = new HashSet<string>(enabledFilters);
        SupportedVersions = new HashSet<uint>(supportedVersions);
    }

    /// <summary>
    /// Configure protocol global settings.
    /// </summary>
    public override void ExportProtocolDefinition(ProtocolDefinition protocolDefinition)
    {
        base.ExportProtocolDefinition(protocolDefinition);

        protocolDefinition.ContractAssembly = _contractAssembly;

        foreach (var version in SupportedVersions)
            protocolDefinition.SupportedVersions.Add(version);

        foreach (var filter in EnabledFilters)
            protocolDefinition.EnabledFilters.Add(filter);
    }
}