using System.Collections.Specialized;
using System.Linq;

namespace XRoadLib.Extensions.AspNet
{
    public static class XRoadRequestExtensions
    {
        public static string GetContentTypeHeader(this NameValueCollection headers)
        {
            var contentTypeKey = headers?.AllKeys.FirstOrDefault(key => key.Trim().ToLower().Equals("content-type"));
            return contentTypeKey == null ? "text/xml; charset=UTF-8" : headers[contentTypeKey];
        }
    }
}