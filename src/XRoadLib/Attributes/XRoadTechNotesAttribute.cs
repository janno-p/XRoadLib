﻿namespace XRoadLib.Attributes;

/// <summary>
/// Description of the service (for developers).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true)]
public class XRoadTechNotesAttribute : Attribute
{
    public string LanguageCode { get; }
    public string Value { get; }

    [UsedImplicitly]
    public XRoadTechNotesAttribute(string value)
    {
        Value = value;
    }

    [UsedImplicitly]
    public XRoadTechNotesAttribute(string languageCode, string value)
    {
        LanguageCode = languageCode;
        Value = value;
    }
}