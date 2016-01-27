using System;
using System.Xml;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Handler.Events
{
    public class BeforeDeserializationEventArgs : EventArgs
    {
        public IServiceMap ServiceMap { get; }
        public SerializationContext Context { get; }
        public XmlReaderSettings XmlReaderSettings { get; set; }

        public BeforeDeserializationEventArgs(SerializationContext context, IServiceMap serviceMap)
        {
            Context = context;
            ServiceMap = serviceMap;
        }
    }
}