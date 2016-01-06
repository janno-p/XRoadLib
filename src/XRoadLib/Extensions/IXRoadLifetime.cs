namespace XRoadLib.Extensions
{
    public interface IXRoadLifetime
    {
        uint? AddedInVersion { get; }
        uint? RemovedInVersion { get; }
    }
}