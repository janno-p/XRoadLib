namespace XRoadLib.Schema;

public enum DefinitionState
{
    /// <summary>
    /// Specifies that definition will appear in service description,
    /// and is allowed to use in request messages.
    /// </summary>
    Default,

    /// <summary>
    /// Specifies that definition will not appears in service description,
    /// but is allowed to use in requests messages.
    /// </summary>
    Hidden,

    /// <summary>
    /// Specifies that definition will not appear in service description,
    /// and is not allowed to use in request messages.
    /// </summary>
    Ignored
}