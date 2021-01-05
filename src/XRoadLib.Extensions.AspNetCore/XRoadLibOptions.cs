using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace XRoadLib.Extensions.AspNetCore
{
    public class XRoadLibOptions
    {
        public DirectoryInfo StoragePath { get; private set; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public XRoadLibOptions WithStoragePath(DirectoryInfo path)
        {
            StoragePath = path;
            return this;
        }
    }
}