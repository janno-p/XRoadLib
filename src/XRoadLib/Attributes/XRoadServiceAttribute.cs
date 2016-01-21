using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class XRoadServiceAttribute : Attribute
    {
        internal uint? addedInVersion;
        internal uint? removedInVersion;

        public string Name { get; }

        public bool IsAbstract { get; set; }
        public bool IsHidden { get; set; }

        public uint AddedInVersion { get { return addedInVersion.GetValueOrDefault(1u); } set { addedInVersion = value; } }
        public uint RemovedInVersion { get { return removedInVersion.GetValueOrDefault(uint.MaxValue); } set { removedInVersion = value; } }

        public XRoadServiceAttribute(string name)
        {
            Name = name;
        }
    }
}