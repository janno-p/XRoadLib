using System;
using XRoadLib.Extensions;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class XRoadServiceAttribute : Attribute, IXRoadLifetime
    {
        private uint? addedInVersion;
        private uint? removedInVersion;

        public string Name { get; }
        public bool IsHidden { get; }

        public uint AddedInVersion { get { return addedInVersion.GetValueOrDefault(1u); } set { addedInVersion = value; } }
        public uint RemovedInVersion { get { return removedInVersion.GetValueOrDefault(uint.MaxValue); } set { removedInVersion = value; } }

        uint? IXRoadLifetime.AddedInVersion => addedInVersion;
        uint? IXRoadLifetime.RemovedInVersion => removedInVersion;

        public XRoadServiceAttribute(string name, bool hidden = false)
        {
            Name = name;
            IsHidden = hidden;
        }
    }
}