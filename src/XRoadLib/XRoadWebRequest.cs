using System.Net;
using XRoadLib.Serialization;

namespace XRoadLib;

/// <summary>
/// Helper methods to initialize X-Road web requests.
/// </summary>
[UsedImplicitly]
public static class XRoadWebRequest
{
    /// <summary>
    /// Initializes new X-Road web request object with default X-Road specific parameter values.
    /// </summary>
    /// <param name="requestUri">Uri of the X-Road security server or adapter server.</param>
    /// <returns>Initialized X-Road web request object.</returns>
    [UsedImplicitly]
    public static WebRequest Create(Uri requestUri)
    {
        var request = WebRequest.Create(requestUri);

        request.ContentType = $"text/xml; charset={XRoadEncoding.Utf8.WebName}";
        request.Headers["SOAPAction"] = string.Empty;
        request.Method = "POST";

        return request;
    }
}