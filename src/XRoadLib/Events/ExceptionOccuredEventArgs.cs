using System;
using XRoadLib.Soap;

namespace XRoadLib.Events
{
    public delegate void ExceptionOccuredEventHandler(object sender, ExceptionOccuredEventArgs e);

    public class ExceptionOccuredEventArgs : EventArgs
    {
        public FaultCode Code { get; set; }
        public string Message { get; set; }
        public string Actor { get; set; }
        public string Detail { get; set; }
        public Exception Exception { get; }

        public ExceptionOccuredEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}