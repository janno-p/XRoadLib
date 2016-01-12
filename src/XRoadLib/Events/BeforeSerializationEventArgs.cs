using System;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Events
{
    public delegate void BeforeSerializationEventHandler(XRoadHttpDataRequest sender, BeforeSerializationEventArgs e);

    public class BeforeSerializationEventArgs : EventArgs
    {
        public SerializationContext Context { get; }
        public object Result { get; }

        public BeforeSerializationEventArgs(object result, SerializationContext context)
        {
            Context = context;
            Result = result;
        }
    }
}