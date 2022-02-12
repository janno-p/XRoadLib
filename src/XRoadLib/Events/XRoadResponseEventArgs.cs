using System.Net;

namespace XRoadLib.Events;

/// <summary>
/// Wraps WebResponse object to be used in event handler.
/// </summary>
public class XRoadResponseEventArgs : EventArgs
{
    /// <summary>
    /// WebResponse object which is returned from invoking the X-Road request.
    /// </summary>
    [UsedImplicitly]
    public WebResponse WebResponse { get; }

    /// <summary>
    /// Content of the response message.
    /// </summary>
    [UsedImplicitly]
    public Stream Stream { get; }

    /// <summary>
    /// Initialize event argument class.
    /// </summary>
    public XRoadResponseEventArgs(WebResponse webResponse, Stream stream)
    {
        Stream = stream;
        WebResponse = webResponse;
    }
}