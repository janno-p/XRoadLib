using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = true)]
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