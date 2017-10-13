using System;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Title of the service (for displaying to users)
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true)]
    public class XRoadTitleAttribute : Attribute
    {
        public string LanguageCode { get; }
        public string Value { get; }

        public XRoadTitleAttribute(string value)
        {
            Value = value;
        }

        public XRoadTitleAttribute(string languageCode, string value)
        {
            LanguageCode = languageCode;
            Value = value;
        }
    }
}