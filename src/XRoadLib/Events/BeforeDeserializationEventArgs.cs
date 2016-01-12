using System;
using System.Reflection;
using System.Xml;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Events
{
    public delegate void BeforeDeserializationEventHandler(XRoadHttpDataRequest sender, BeforeDeserializationEventArgs e);

    public class BeforeDeserializationEventArgs : EventArgs
    {
        public MethodInfo MethodInfo { get; }
        public SerializationContext Context { get; }
        public XmlReaderSettings XmlReaderSettings { get; set; }

        public BeforeDeserializationEventArgs(SerializationContext context, MethodInfo methodInfo)
        {
            Context = context;
            MethodInfo = methodInfo;
        }
    }
}