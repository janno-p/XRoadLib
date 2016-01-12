using System;

namespace XRoadLib.Events
{
    public delegate void InvocationErrorHandler(XRoadHttpDataRequest sender, InvocationErrorEventArgs e);

    public class InvocationErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public object Result { get; set; }

        public InvocationErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}