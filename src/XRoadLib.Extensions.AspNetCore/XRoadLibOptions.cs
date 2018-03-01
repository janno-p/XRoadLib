using System.IO;

namespace XRoadLib.Extensions.AspNetCore
{
    public class XRoadLibOptions
    {
        public DirectoryInfo StoragePath { get; private set; }

        public XRoadLibOptions WithStoragePath(DirectoryInfo path)
        {
            StoragePath = path;
            return this;
        }
    }
}