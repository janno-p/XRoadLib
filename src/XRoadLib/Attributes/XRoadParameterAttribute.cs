using System;
using XRoadLib.Extensions;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class XRoadParameterAttribute : Attribute, IXRoadLifetime
    {
        private uint? addedInVersion;
        private uint? removedInVersion;

        public string Name { get; set; }
        public bool IsOptional { get; set; }

        public uint AddedInVersion { get { return addedInVersion.GetValueOrDefault(); } set { addedInVersion = value; } }
        public uint RemovedInVersion { get { return removedInVersion.GetValueOrDefault(uint.MaxValue); } set { removedInVersion = value; } }

        uint? IXRoadLifetime.AddedInVersion => addedInVersion;
        uint? IXRoadLifetime.RemovedInVersion => removedInVersion;
    }
}