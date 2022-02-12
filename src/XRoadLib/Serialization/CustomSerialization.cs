namespace XRoadLib.Serialization;

[UsedImplicitly]
public class CustomSerialization : ICustomSerialization
{
    public virtual Task OnContentCompleteAsync(XmlWriter writer) =>
        Task.CompletedTask;
}