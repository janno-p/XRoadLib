using System;

namespace XRoadLib.Events
{
    public delegate void AfterSerializationEventHandler(XRoadHttpDataRequest sender, AfterSerializationEventArgs e);

    public class AfterSerializationEventArgs : EventArgs
    {
        public object Result { get; }

        public AfterSerializationEventArgs(object result)
        {
            Result = result;
        }
    }
}