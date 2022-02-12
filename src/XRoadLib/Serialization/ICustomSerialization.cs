namespace XRoadLib.Serialization;

public interface ICustomSerialization
{
    [UsedImplicitly]
    Task OnContentCompleteAsync(XmlWriter writer);
}