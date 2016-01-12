using System;
using XRoadLib.Serialization;

namespace XRoadLib.Events
{
    public class MessageEventArgs : EventArgs
    {
        public XRoadMessage Message { get; }

        public MessageEventArgs(XRoadMessage message)
        {
            Message = message;
        }
    }
}