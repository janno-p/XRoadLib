using System.Net;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib;

/// <summary>
/// Manages available services and provides their definitions and serialization details.
/// </summary>
public class ServiceManager<THeader> : ServiceManager
    where THeader : class, ISoapHeader, new()
{
    /// <inheritdoc cref="ServiceManager" />
    public ServiceManager(string name, ISchemaExporter schemaExporter)
        : base(name, schemaExporter)
    { }

    /// <summary>
    /// Executes X-Road operation on endpoint specified by WebRequest parameter.
    /// </summary>
    /// <param name="webRequest">WebRequest used to transfer X-Road messages.</param>
    /// <param name="body">Soap body part of outgoing serialized X-Road message.</param>
    /// <param name="header">Soap header part of outgoing serialized X-Road message.</param>
    /// <param name="options">Additional options to configure service call execution.</param>
    /// <typeparam name="TResult">Expected result type of the operation.</typeparam>
    /// <returns>Deserialized value of X-Road response message Soap body.</returns>
    [UsedImplicitly]
    public virtual async Task<TResult> ExecuteAsync<TResult>(WebRequest webRequest, object body, THeader header, ServiceExecutionOptions? options = null) =>
        (TResult)await ((IServiceManager)this).ExecuteAsync(webRequest, body, header, options).ConfigureAwait(false);

    /// <summary>
    /// Initialize new X-Road message of this X-Road message protocol instance.
    /// </summary>
    public XRoadMessage CreateMessage(THeader? header = null) =>
        new(this, header ?? new THeader());

    protected override ISoapHeader CreateHeader() =>
        new THeader();
}