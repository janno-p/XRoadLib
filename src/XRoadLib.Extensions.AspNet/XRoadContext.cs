using System;
using System.IO;
using System.Web;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNet
{
    /// <summary>
    /// X-Road context of classical ASP.NET applications.
    /// </summary>
    public class XRoadContext : IDisposable
    {
        /// <summary>
        /// HTTP context this X-Road context is bound to.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// X-Road request message.
        /// </summary>
        public XRoadMessage Request { get; }

        /// <summary>
        /// X-Road response message.
        /// </summary>
        public XRoadMessage Response { get; }

        /// <summary>
        /// ServiceMap which maps to operation of the message.
        /// </summary>
        public IServiceMap ServiceMap { get; set; }

        /// <summary>
        /// Deserialized request parameters.
        /// </summary>
        public object Parameters { get; set; }

        /// <summary>
        /// Result of the X-Road service request.
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Exception that occured while handling service request.
        /// </summary>
        public Exception Exception { get; set; }

        public IMessageFormatter MessageFormatter { get; set; }

        /// <summary>
        /// Initialize new X-Road context instance.
        /// </summary>
        public XRoadContext(HttpContext httpContext)
        {
            HttpContext = httpContext;
            Request = new XRoadMessage();
            Response = new XRoadMessage(new MemoryStream());
        }

        void IDisposable.Dispose()
        {
            Response?.Dispose();
            Request?.Dispose();
        }
    }
}