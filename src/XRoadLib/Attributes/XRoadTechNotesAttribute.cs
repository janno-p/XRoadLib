using System;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Description of the service (for developers).
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true)]
    public class XRoadTechNotesAttribute : Attribute
    {
        public string LanguageCode { get; }
        public string Value { get; }

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