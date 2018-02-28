using System.Linq;
using Microsoft.AspNetCore.Http;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadRequestExtensions
    {
        public static string GetContentTypeHeader(this IHeaderDictionary headers)
        {
            var contentTypeKey = headers?.Keys.FirstOrDefault(key => key.Trim().ToLower().Equals("content-type"));
            return contentTypeKey == null ? "text/xml; charset=UTF-8" : headers?[contentTypeKey].ToString();
        }
    }
}