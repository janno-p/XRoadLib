using System;
using Microsoft.AspNetCore.Http;

namespace XRoadLib.Extensions.AspNetCore
{
    [AttributeUsage(AttributeTargets.Class)]
    public class XRoadHandlerAttribute : Attribute
    {
        public string Route { get; }
        public string HttpMethod { get; }
        
        public XRoadHandlerAttribute(string route, string httpMethod)
        {
            Route = route.GetValueOrDefault("/");
            HttpMethod = httpMethod.GetValueOrDefault(HttpMethods.Get);
        }
    }
}