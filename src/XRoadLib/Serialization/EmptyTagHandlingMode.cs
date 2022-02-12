﻿namespace XRoadLib.Serialization;

/// <summary>
/// Specifies empty tag handling behavior for xml deserialization.
/// </summary>
public enum EmptyTagHandlingMode
{
    /// <summary>
    /// Follows Xml Schema specification and produces errors when empty tag is invalid value.
    /// </summary>
    [UsedImplicitly] Strict,

    /// <summary>
    /// Empty tag produces default value of the property type.
    /// </summary>
    DefaultValue,

    /// <summary>
    /// Empty tag produces value `null` for nullable value types and default value for
    /// non nullable types.
    /// </summary>
    Null
}