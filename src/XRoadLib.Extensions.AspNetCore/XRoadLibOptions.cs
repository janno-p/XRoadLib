using JetBrains.Annotations;

namespace XRoadLib.Extensions.AspNetCore;

public class XRoadLibOptions
{
    public DirectoryInfo? StoragePath { get; private set; }

    [UsedImplicitly]
    public XRoadLibOptions WithStoragePath(DirectoryInfo path)
    {
        StoragePath = path;
        return this;
    }
}