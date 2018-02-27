using System;
using System.IO;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    /// <summary>
    /// Combines X-Road message protocol request and response into single object.
    /// </summary>
    public abstract class XRoadContext<THttpContext> : IDisposable
    {
        /// <summary>
        /// HTTP context this X-Road context is bound to.
        /// </summary>
        public THttpContext HttpContext { get; }

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

        /// <summary>
        /// Intializes new X-Road message context.
        /// </summary>
        public XRoadContext(THttpContext httpContext)
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

#if !NET452

    /// <summary>
    /// X-Road context of classical AspNetCore applications.
    /// </summary>
    public class XRoadContext : XRoadContext<Microsoft.AspNetCore.Http.HttpContext>
    {
        /// <summary>
        /// Initialize new X-Road context instance.
        /// </summary>
        public XRoadContext(Microsoft.AspNetCore.Http.HttpContext httpContext)
            : base(httpContext)
        { }
    }
#endif

#if !NETSTANDARD2_0
    /// <summary>
    /// X-Road context of classical ASP.NET applications.
    /// </summary>
    public class XRoadContextClassic : XRoadContext<System.Web.HttpContext>
    {
        /// <summary>
        /// Initialize new X-Road context instance.
        /// </summary>
        public XRoadContextClassic(System.Web.HttpContext httpContext)
            : base(httpContext)
        { }
    }
#endif
}