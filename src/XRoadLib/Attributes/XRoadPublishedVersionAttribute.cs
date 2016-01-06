using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class XRoadPublishedVersionAttribute : Attribute
    {
        private uint? firstVersion;

        public uint FirstVersion { get { return firstVersion.GetValueOrDefault(1u); } set { firstVersion = value; } }
        public uint Version { get; } 

        public XRoadPublishedVersionAttribute(uint version)
        {
            Version = version;
        }
    }
}