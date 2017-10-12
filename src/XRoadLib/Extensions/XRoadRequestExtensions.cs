using System.Linq;
using System.Net;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#else
using System.Collections.Specialized;
#endif

namespace XRoadLib.Extensions
{
    public static class XRoadRequestExtensions
    {
#if NETSTANDARD2_0

        public static string GetContentTypeHeader(this IHeaderDictionary headers)
        {
            var contentTypeKey = headers?.Keys.FirstOrDefault(key => key.Trim().ToLower().Equals("content-type"));
            return contentTypeKey == null ? "text/xml; charset=UTF-8" : headers?[contentTypeKey].ToString();
        }

#else

        public static string GetContentTypeHeader(this NameValueCollection headers)
        {
            var contentTypeKey = headers?.AllKeys.FirstOrDefault(key => key.Trim().ToLower().Equals("content-type"));
            return contentTypeKey == null ? "text/xml; charset=UTF-8" : headers?[contentTypeKey];
        }

#endif

        public static string GetContentTypeHeader(this WebHeaderCollection headers)
        {
            var contentTypeKey = headers?.AllKeys.FirstOrDefault(key => key.Trim().ToLower().Equals("content-type"));
            return contentTypeKey == null ? "text/xml; charset=UTF-8" : headers?[contentTypeKey].ToString();
        }
    }
}