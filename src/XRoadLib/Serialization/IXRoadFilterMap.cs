namespace XRoadLib.Serialization;

public interface IXRoadFilterMap
{
    string GroupName { get; }

    ISet<string> EnabledProperties { get; }
}