using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class XRoadPublishedVersionAttribute : Attribute
    {
        public uint Version { get; }

        public XRoadPublishedVersionAttribute(uint version)
        {
            Version = version;
        }
    }
}