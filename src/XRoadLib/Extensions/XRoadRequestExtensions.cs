using System.Net;

namespace XRoadLib.Extensions;

public static class XRoadRequestExtensions
{
    public static string GetContentTypeHeader(this WebHeaderCollection headers)
    {
        var contentTypeKey = headers?.AllKeys.FirstOrDefault(key => key.Trim().ToLower().Equals("content-type"));
        return contentTypeKey == null ? "text/xml; charset=UTF-8" : headers[contentTypeKey];
    }
}