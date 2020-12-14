using System;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Description of the service (for developers).
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public class XRoadTechNotesAttribute : Attribute
    {
        public string LanguageCode { get; }
        public string Value { get; }

        public DocumentationTarget Target { get; set; } = DocumentationTarget.Default;

        public XRoadTechNotesAttribute(string value)
        {
            Value = value;
        }

        public XRoadTechNotesAttribute(string languageCode, string value)
        {
            LanguageCode = languageCode;
            Value = value;
        }
    }
}