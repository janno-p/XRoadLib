using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class XRoadAttachmentAttribute : Attribute
    {
        public bool HasMultipartRequest { get; set; }
        public bool HasMultipartResponse { get; set; }
    }
}