using System;
using System.Net;
using XRoadLib.Serialization;

namespace XRoadLib.Events
{
    /// <summary>
    /// Wraps WebRequest object to be used in event handler.
    /// </summary>
    public class XRoadRequestEventArgs : EventArgs
    {
        /// <summary>
        /// WebRequest object which is used to invoke X-Road request.
        /// </summary>
        public WebRequest WebRequest { get; }

        /// <summary>
        /// X-Road message object that is about to be serialized to WebRequest.
        /// </summary>
        public XRoadMessage Message { get; }

        /// <summary>
        /// Initialize event argument class.
        /// </summary>
        public XRoadRequestEventArgs(WebRequest webRequest, XRoadMessage message)
        {
            Message = message;
            WebRequest = webRequest;
        }
    }
}