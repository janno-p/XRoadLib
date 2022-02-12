using System.Security.Cryptography;

namespace XRoadLib.Serialization;

/// <summary>
/// Binary content of X-Road messages.
/// </summary>
public sealed class XRoadAttachment : IDisposable
{
    private readonly string _contentPath;

    /// <summary>
    /// Underlying stream which contains the content.
    /// </summary>
    public Stream ContentStream { get; private set; }

    /// <summary>
    /// Indicates whether content is serialized as multipart attachement or not.
    /// </summary>
    public bool IsMultipartContent { get; set; }

    /// <summary>
    /// Attachments unique identificator inside the request
    /// which references the multipart content.
    /// </summary>
    public string ContentId { get; }

    /// <summary>
    /// Does the attachment have any content.
    /// </summary>
    [UsedImplicitly]
    public bool HasContent => ContentStream.Length > 0;

    private XRoadAttachment()
    {
        IsMultipartContent = true;
    }

    /// <summary>
    /// Initializes new attachment based on existing stream.
    /// </summary>
    public XRoadAttachment(byte[] contentBytes) : this()
    {
        var contentStream = new MemoryStream(contentBytes);
        ContentId = Convert.ToBase64String(MD5.Create().ComputeHash(contentStream));
        ContentStream = contentStream;
    }

    /// <summary>
    /// Initializes new attachment based on existing stream.
    /// </summary>
    public XRoadAttachment(Stream contentStream) : this()
    {
        ContentId = Convert.ToBase64String(MD5.Create().ComputeHash(contentStream));
        ContentStream = contentStream;
    }

    /// <summary>
    /// Initializes new attachment from temporary file, which is specified
    /// by fullPath.
    /// </summary>
    public XRoadAttachment(string contentId, string fullPath) : this()
    {
        ContentStream = new FileStream(fullPath, FileMode.Create);

        if (string.IsNullOrEmpty(contentId))
            contentId = Convert.ToBase64String(MD5.Create().ComputeHash(ContentStream));

        ContentId = contentId;
        _contentPath = fullPath;
    }

    private void Close()
    {
        ContentStream?.Dispose();
        ContentStream = null;

        // kui path on teada, siis on temp fail ja see tuleb kustutada ..
        if (!string.IsNullOrEmpty(_contentPath) && File.Exists(_contentPath))
            File.Delete(_contentPath);
    }

    /// <summary>
    /// Writes attachments contents to the specificed XML writer object
    /// using base64 encoding.
    /// </summary>
    public async Task WriteAsBase64Async(XmlWriter writer)
    {
        ContentStream.Position = 0;

        const int bufferSize = 1000;

        int bytesRead;
        var buffer = new byte[bufferSize];

        while ((bytesRead = await ContentStream.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false)) > 0)
            await writer.WriteBase64Async(buffer, 0, bytesRead).ConfigureAwait(false);
    }

    /// <summary>
    /// Clean up unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Close();
    }
}