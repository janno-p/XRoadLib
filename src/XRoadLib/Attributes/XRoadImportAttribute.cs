using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class XRoadImportAttribute : Attribute
    {
        public string RequestPart { get; }
        public string ResponsePart { get; }

        public XRoadImportAttribute(string requestPart, string responsePart)
        {
            RequestPart = requestPart;
            ResponsePart = responsePart;
        }
    }
}