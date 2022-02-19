using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Attributes;

/// <summary>
/// Defines operation method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class XRoadServiceAttribute : Attribute
{
    internal uint? AddedInVersionValue;
    internal uint? RemovedInVersionValue;

    /// <summary>
    /// Operation name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// ServiceMap type which implements operation definition.
    /// </summary>
    public virtual Type ServiceMapType { get; } = typeof(ServiceMap);

    /// <summary>
    /// Abstract operations do not define binding details.
    /// </summary>
    [UsedImplicitly]
    public virtual bool IsAbstract { get; set; }

    /// <summary>
    /// Hidden operations are not included in service description.
    /// </summary>
    [UsedImplicitly]
    public virtual bool IsHidden { get; set; }

    /// <summary>
    /// X-Road service version which first defined given operation.
    /// </summary>
    [UsedImplicitly]
    public virtual uint AddedInVersion { get => AddedInVersionValue.GetValueOrDefault(1u); set => AddedInVersionValue = value; }

    /// <summary>
    /// X-Road service version which removed given operation.
    /// </summary>
    [UsedImplicitly]
    public virtual uint RemovedInVersion { get => RemovedInVersionValue.GetValueOrDefault(uint.MaxValue); set => RemovedInVersionValue = value; }

    /// <summary>
    /// Provides extension specific customizations for the schema.
    /// </summary>
    public virtual ISchemaExporter? SchemaExporter => null;

    /// <summary>
    /// Attachment serialization mode for service input. Available options are `Xml` (binary is serialized inside XML document
    /// using base64 encoding or MTOM optimization) or `Attachment` (binary is serialized as mime multipart attachment)
    /// </summary>
    [UsedImplicitly]
    public virtual BinaryMode InputBinaryMode { get; set; } = BinaryMode.Xml;

    /// <summary>
    /// Attachment serialization mode for service output. Available options are `Xml` (binary is serialized inside XML document
    /// using base64 encoding or MTOM optimization) or `Attachment` (binary is serialized as mime multipart attachment)
    /// </summary>
    [UsedImplicitly]
    public virtual BinaryMode OutputBinaryMode { get; set; }= BinaryMode.Xml;

    /// <summary>
    /// SOAPAction header value for this service.
    /// </summary>
    [UsedImplicitly]
    public virtual string SoapAction { get; set; } = string.Empty;

    /// <summary>
    /// Initializes new operation definition.
    /// </summary>
    public XRoadServiceAttribute(string name)
    {
        Name = name;
    }
}