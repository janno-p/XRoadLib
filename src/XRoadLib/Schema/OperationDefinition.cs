using System.Reflection;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema;

/// <summary>
/// Configurable part of operation.
/// </summary>
public class OperationDefinition : Definition
{
    /// <summary>
    /// Runtime interface method which represents current operation.
    /// </summary>
    public MethodInfo MethodInfo { get; }

    /// <summary>
    /// X-Road service version of the operation.
    /// </summary>
    [UsedImplicitly]
    public uint Version { get; set; }

    /// <summary>
    /// Should this operation define binding part in service description?
    /// </summary>
    [UsedImplicitly]
    public bool IsAbstract { get; set; }

    /// <summary>
    /// Override input message name.
    /// </summary>
    [UsedImplicitly]
    public string InputMessageName { get; set; }

    /// <summary>
    /// Binary content serialization format for input messages.
    /// </summary>
    [UsedImplicitly]
    public BinaryMode InputBinaryMode { get; set; }

    /// <summary>
    /// Does the operation return input message as part of output message.
    /// </summary>
    public bool CopyRequestPartToResponse { get; set; }

    /// <summary>
    /// Override output message name.
    /// </summary>
    [UsedImplicitly]
    public string OutputMessageName { get; set; }

    /// <summary>
    /// Binary content serialization format for output messages.
    /// </summary>
    [UsedImplicitly]
    public BinaryMode OutputBinaryMode { get; set; }

    /// <summary>
    /// Serialization functionality of current operation.
    /// </summary>
    [UsedImplicitly]
    public Type ServiceMapType { get; set; }

    /// <summary>
    /// Customized export options defined by extension.
    /// </summary>
    public ISchemaExporter? ExtensionSchemaExporter { get; }

    /// <summary>
    /// Customized SOAPAction header value for this operation.
    /// </summary>
    [UsedImplicitly]
    public string SoapAction { get; set; }

    /// <summary>
    /// Initializes new definition object using default settings.
    /// </summary>
    public OperationDefinition(XName qualifiedName, uint? version, MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;

        var attribute = methodInfo.GetServices().SingleOrDefault(x => x.Name == qualifiedName.LocalName);

        Name = qualifiedName;
        IsAbstract = (attribute?.IsAbstract).GetValueOrDefault();
        InputBinaryMode = (attribute?.InputBinaryMode).GetValueOrDefault(BinaryMode.Xml);
        OutputBinaryMode = (attribute?.OutputBinaryMode).GetValueOrDefault(BinaryMode.Xml);
        State = (attribute?.IsHidden).GetValueOrDefault() ? DefinitionState.Hidden : DefinitionState.Default;
        Version = version.GetValueOrDefault(attribute?.AddedInVersion ?? 0u);
        CopyRequestPartToResponse = true;
        InputMessageName = qualifiedName.LocalName;
        OutputMessageName = $"{qualifiedName.LocalName}Response";
        Documentation = new DocumentationDefinition(methodInfo);
        ServiceMapType = attribute?.ServiceMapType ?? typeof(ServiceMap);
        ExtensionSchemaExporter = attribute?.SchemaExporter;
        SoapAction = attribute?.SoapAction ?? string.Empty;
    }
}