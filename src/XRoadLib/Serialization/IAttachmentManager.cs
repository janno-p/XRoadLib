namespace XRoadLib.Serialization;

public interface IAttachmentManager : IDisposable
{
    [UsedImplicitly]
    XRoadAttachment GetAttachment(string contentId);

    IList<XRoadAttachment> AllAttachments { get; }

    [UsedImplicitly]
    IEnumerable<XRoadAttachment> MultipartContentAttachments { get; }
}