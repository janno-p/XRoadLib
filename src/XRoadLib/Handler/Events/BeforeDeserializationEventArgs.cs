using System;
using System.Xml;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Handler.Events
{
    public class BeforeDeserializationEventArgs : EventArgs
    {
        public IServiceMap ServiceMap { get; }
        public XmlReaderSettings XmlReaderSettings { get; set; }

        public BeforeDeserializationEventArgs(IServiceMap serviceMap)
        {
            ServiceMap = serviceMap;
        }
    }
}