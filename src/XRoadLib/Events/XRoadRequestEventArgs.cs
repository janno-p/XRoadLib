using System;
using System.Diagnostics.CodeAnalysis;
using XRoadLib.Serialization;

namespace XRoadLib.Events
{
    /// <summary>
    /// Wraps WebRequest object to be used in event handler.
    /// </summary>
    public class XRoadRequestEventArgs : EventArgs
    {
        /// <summary>
        /// X-Road message object that is about to be serialized to WebRequest.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public XRoadMessage Message { get; }

        /// <summary>
        /// Initialize event argument class.
        /// </summary>
        public XRoadRequestEventArgs(XRoadMessage message)
        {
            Message = message;
        }
    }
}