namespace XRoadLib.Serialization;

public static class ContentTypes
{
    /// <summary>
    /// Content-type of SOAP 1.1 protocol messages.
    /// </summary>
    public const string Soap = "text/xml";

    /// <summary>
    /// Content-type of SOAP 1.2 protocol messages.
    /// </summary>
    public const string Soap12 = "application/soap+xml";

    /// <summary>
    /// Content-type of messages with mime/multipart optimization.
    /// </summary>
    public const string Xop = "application/xop+xml";

    /// <summary>
    /// Content-type of mime/multipart messages.
    /// </summary>
    public const string Multipart = "multipart/related";
}