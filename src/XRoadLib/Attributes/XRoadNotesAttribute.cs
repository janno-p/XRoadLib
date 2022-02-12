namespace XRoadLib.Attributes;

/// <summary>
/// Description of the service (for displaying to users).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true)]
public class XRoadNotesAttribute : Attribute
{
    public string LanguageCode { get; }
    public string Value { get; }

    [UsedImplicitly]
    public XRoadNotesAttribute(string value)
    {
        Value = value;
    }

    public XRoadNotesAttribute(string languageCode, string value)
    {
        LanguageCode = languageCode;
        Value = value;
    }
}